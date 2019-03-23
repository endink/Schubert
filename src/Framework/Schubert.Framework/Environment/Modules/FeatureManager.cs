using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Schubert.Framework.Caching;
using Schubert.Framework.Environment.Modules.Loaders;
using Schubert.Framework.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Schubert.Framework.Environment.Modules
{
    public class FeatureManager : IFeatureManager
    {
        private readonly IModuleManager _moduleManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private IEnumerable<IModuleLoader> _loaders = null;

        /// <summary>
        /// Delegate to notify about feature dependencies.
        /// </summary>
        public event FeatureDependencyNotificationHandler FeatureDependencyNotification;

        public FeatureManager(
            IModuleManager moduleManager,
            IShellDescriptorManager shellDescriptorManager,
            IEnumerable<IModuleLoader> loaders,
            ILoggerFactory loggerFactory)
        {
            Guard.ArgumentNotNull(moduleManager, nameof(moduleManager));
            Guard.ArgumentNotNull(shellDescriptorManager, nameof(shellDescriptorManager));
            Guard.ArgumentNotNull(loaders, nameof(loaders));

            _moduleManager = moduleManager;
            _shellDescriptorManager = shellDescriptorManager;
            _loaders = loaders;
            
            Logger = loggerFactory?.CreateLogger<FeatureManager>() ?? (ILogger)NullLogger.Instance;
        }
        public ILogger Logger { get; set; }
        
       
        private IEnumerable<FeatureDescriptor> GetAvailableFeatureDescriptors()
        {
            return _moduleManager.GetAvailableFeatures();
        }

        public IEnumerable<Feature> GetAvailableFeatures()
        {
            return this.GetAvailableFeatureDescriptors().Select(this.LoadFeature);
        }

        public IEnumerable<Feature> GetEnabledFeatures()
        {
            var currentShellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            if (currentShellDescriptor == null) // 当存储中找不到 Shell 信息时（可能初始化系统）我们应该启用所有模块。
            {
                var allFeatures = this.GetAvailableFeatures();
                _shellDescriptorManager.UpdateShellDescriptor(Enumerable.Empty<String>(), Enumerable.Empty<ShellParameter>());
                return allFeatures;
            }
            return _moduleManager.GetEnabledFeatures(currentShellDescriptor).Select(this.LoadFeature);
        }

        private static string GetSourceFeatureNameForType(Type type)
        {
            var attributes = type.GetTypeInfo().GetCustomAttributes(typeof(SchubertFeatureAttribute), false);
            foreach (SchubertFeatureAttribute featureAttribute in attributes)
            {
                return featureAttribute.FeatureName;
            }
            return String.Empty;
        }

        private IEnumerable<Type> LoadModuleTypes(ModuleDescriptor module)
        {
            ModuleAccess moduleAccess = null;
            foreach (IModuleLoader loader in _loaders)
            {
                this.Logger.WriteTrace($"尝试加载模块 {module.Name}，加载程序：{loader.GetType().Name}。{System.Environment.NewLine}Location：{module.LibraryPath}{System.Environment.NewLine}Configuration：{module.ModuleManifest}");
                bool result = loader.TryLoad(module, out moduleAccess);
                if (result)
                {
                    this.Logger.WriteTrace($"{loader.GetType().Name} 加载模块 {module.Name} 成功。");
                    break;
                }
                else
                {
                    this.Logger.WriteTrace($"{loader.GetType().Name} 加载模块 {module.Name} 失败。");
                }
            }

            if (moduleAccess == null)
            {
                return Enumerable.Empty<Type>();
            }

            return moduleAccess.ExportedTypes;

        }

        private Feature LoadFeature(FeatureDescriptor featureDescriptor)
        {
            return LocalCache.Current.GetOrSet(featureDescriptor.Name, key =>
            {
                var featureName = featureDescriptor.Name;
                var moduleName = featureDescriptor.ModuleName;
                var featureTypes = new List<Type>();
                if (!moduleName.IsNullOrWhiteSpace())
                {
                    var module = _moduleManager.GetModule(featureDescriptor.ModuleName);
                    if (module != null)
                    {
                        IEnumerable<Type> moduleTypes = this.LoadModuleTypes(module);

                        foreach (var type in moduleTypes)
                        {
                            string sourceFeature = GetSourceFeatureNameForType(type).IfNullOrWhiteSpace(featureDescriptor.ModuleName);
                            if (String.Equals(sourceFeature, featureName, StringComparison.OrdinalIgnoreCase))
                            {
                                featureTypes.Add(type);
                            }
                        }
                    }
                }

                // If the feature could not be compiled for some reason,
                // return a "null" feature, i.e. a feature with no exported types.
                return new Feature
                {
                    Descriptor = featureDescriptor,
                    ExportedTypes = featureTypes.AsReadOnly()
                };
            }, TimeSpan.FromMinutes(10));
        }

        


        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureNames">The names for the features to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        public IEnumerable<string> EnableFeatures(IEnumerable<string> featureNames, bool force)
        {
            ShellDescriptor shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            List<String> disabledFeatures = shellDescriptor.DisabledFeatures.ToList();

            IDictionary<FeatureDescriptor, bool> availableFeatures = GetAvailableFeatureDescriptors()
                .ToDictionary(featureDescriptor => featureDescriptor,
                                featureDescriptor => !disabledFeatures.Any(sf => sf == featureDescriptor.Name)); 

            IEnumerable<string> featuresToEnable = featureNames
                .Select(featureId => EnableFeature(featureId, availableFeatures, force)).ToList()
                .SelectMany(ies => ies.Select(s => s));

            if (featuresToEnable.Count() > 0)
            {
                foreach (string featureId in featuresToEnable)
                {
                    string name = featureId;

                    disabledFeatures.Remove(name);
                    Logger.WriteInformation("{0} was enabled", featureId);
                }

                _shellDescriptorManager.UpdateShellDescriptor(disabledFeatures,
                                                              shellDescriptor.Parameters);
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureNames">The names for the features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        /// <returns>An enumeration with the disabled feature names.</returns>
        public IEnumerable<string> DisableFeatures(IEnumerable<string> featureNames, bool force)
        {
            ShellDescriptor shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            List<string> disabledFeatures = shellDescriptor.DisabledFeatures.ToList();

            IEnumerable<string> featuresToDisable = featureNames
                .Select(featureId => DisableFeature(featureId, force)).ToList()
                .SelectMany(ies => ies.Select(s => s));

            if (featuresToDisable.Any())
            {
                foreach (string featureName in featuresToDisable)
                {
                    disabledFeatures.Add(featureName);
                    Logger.WriteInformation("{0} was disabled", featureName);
                }

                _shellDescriptorManager.UpdateShellDescriptor(disabledFeatures,
                                                              shellDescriptor.Parameters);
            }

            return featuresToDisable;
        }

        /// <summary>
        /// Lists all enabled features that depend on a given feature.
        /// </summary>
        /// <param name="featureName">Name of the feature to check.</param>
        /// <returns>An enumeration with dependent feature names.</returns>
        public IEnumerable<string> GetDependentFeatures(string featureName)
        {
            var getEnabledDependants =
                new Func<string, IDictionary<FeatureDescriptor, bool>, IDictionary<FeatureDescriptor, bool>>(
                    (currentFeatureId, fs) => fs
                        .Where(f => f.Value && f.Key.Dependencies != null && f.Key.Dependencies
                            .Select(s => s.ToLower())
                            .Contains(currentFeatureId.ToLower()))
                        .ToDictionary(f => f.Key, f => f.Value));

            ShellDescriptor shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            List<String> disabledFeatures = shellDescriptor.DisabledFeatures.ToList();

            IDictionary<FeatureDescriptor, bool> availableFeatures = GetAvailableFeatureDescriptors()
                .ToDictionary(featureDescriptor => featureDescriptor,
                              featureDescriptor => !disabledFeatures.Any(shellFeature => shellFeature.Equals(featureDescriptor.Name)));

            return GetAffectedFeatures(featureName, availableFeatures, getEnabledDependants);
        }

        /// <summary>
        /// Enables a feature.
        /// </summary>
        /// <param name="featureName">The name of the feature to be enabled.</param>
        /// <param name="availableFeatures">A dictionary of the available feature descriptors and their current state (enabled / disabled).</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration of the enabled features.</returns>
        private IEnumerable<string> EnableFeature(string featureName, IDictionary<FeatureDescriptor, bool> availableFeatures, bool force)
        {
            var getDisabledDependencies =
                new Func<string, IDictionary<FeatureDescriptor, bool>, IDictionary<FeatureDescriptor, bool>>(
                    (currentFeatureName, featuresState) =>
                    {
                        KeyValuePair<FeatureDescriptor, bool> feature = featuresState.Single(featureState => featureState.Key.Name.Equals(currentFeatureName, StringComparison.OrdinalIgnoreCase));

                        // Retrieve disabled dependencies for the current feature
                        return feature.Key.Dependencies
                                      .Select(fName =>
                                      {
                                          var states = featuresState.Where(featureState => featureState.Key.Name.Equals(fName, StringComparison.OrdinalIgnoreCase)).ToList();

                                          if (states.Count == 0)
                                          {
                                              throw new SchubertException("Failed to get state for feature {0}", fName);
                                          }

                                          if (states.Count > 1)
                                          {
                                              throw new SchubertException("Found {0} states for feature {1}", states.Count, fName);
                                          }

                                          return states[0];
                                      })
                                      .Where(featureState => !featureState.Value)
                                      .ToDictionary(f => f.Key, f => f.Value);
                    });

            IEnumerable<string> featuresToEnable = GetAffectedFeatures(featureName, availableFeatures, getDisabledDependencies);
            if (featuresToEnable.Count() > 1 && !force)
            {
                Logger.WriteWarning("Additional features need to be enabled.");
                if (FeatureDependencyNotification != null)
                {
                    FeatureDependencyNotification("If {0} is enabled, then you'll also need to enable {1}.", featureName, featuresToEnable.Where(fId => fId != featureName));
                }

                return Enumerable.Empty<string>();
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a feature.
        /// </summary>
        /// <param name="featureName">The name of the feature to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration of the disabled features.</returns>
        private IEnumerable<string> DisableFeature(string featureName, bool force)
        {
            IEnumerable<string> featuresToDisable = GetDependentFeatures(featureName);

            if (featuresToDisable.Count() > 1 && !force)
            {
                Logger.WriteWarning("Additional features need to be disabled.");
                if (FeatureDependencyNotification != null)
                {
                    FeatureDependencyNotification("If {0} is disabled, then you'll also need to disable {1}.", featureName, featuresToDisable.Where(fId => fId != featureName));
                }

                return Enumerable.Empty<string>();
            }

            return featuresToDisable;
        }

        private static IEnumerable<string> GetAffectedFeatures(
            string featureName,
            IDictionary<FeatureDescriptor, bool> features,
            Func<string, IDictionary<FeatureDescriptor, bool>, IDictionary<FeatureDescriptor, bool>> getAffectedDependencies)
        {

            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { featureName };
            var stack = new Stack<IDictionary<FeatureDescriptor, bool>>();

            stack.Push(getAffectedDependencies(featureName, features));

            while (stack.Any())
            {
                var next = stack.Pop();
                foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency.Key.Name)))
                {
                    dependencies.Add(dependency.Key.Name);
                    stack.Push(getAffectedDependencies(dependency.Key.Name, features));
                }
            }

            return dependencies;
        }
    }
}
