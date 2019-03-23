using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Schubert.Framework.Caching;
using Schubert.Framework.Environment.Modules.Finders;
using Schubert.Framework.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment.Modules
{
    public class ModuleManager : IModuleManager
    {
        private readonly IEnumerable<IModuleFinder> _folders;

        internal const string AvailableModulesCacheName = "AvailableModules";
        internal const string AvailableFeaturesCacheName = "AvailableFeatures";
        
        public ILogger Logger { get; set; }

        public ModuleManager(IEnumerable<IModuleFinder> folders, ILoggerFactory loggerFactory)
        {
            _folders = folders ?? Enumerable.Empty<IModuleFinder>();
            Logger = loggerFactory?.CreateLogger<ModuleManager>() ?? (ILogger)NullLogger.Instance;
        }
        
        public ModuleDescriptor GetModule(string name)
        {
            Guard.ArgumentNullOrWhiteSpaceString(name, nameof(name));
            return this.GetAvailableModules().FirstOrDefault(x => x.Name == name);
        }

        public IEnumerable<ModuleDescriptor> GetAvailableModules()
        {
            return LocalCache.Current.GetOrSet(AvailableModulesCacheName, this.ParallelLoadFolders,  TimeSpan.FromMinutes(10));
        }

        private IEnumerable<ModuleDescriptor> ParallelLoadFolders(string cacheKey)
        {
            List<ModuleDescriptor> list = new List<ModuleDescriptor>();
            Parallel.ForEach(_folders, (folder) =>
            {
                list.AddRange(folder.FindAvailableModules());
            });
            return list.Distinct(m => m.Name).ToArray();
        }

        public IEnumerable<FeatureDescriptor> GetAvailableFeatures()
        {
            return LocalCache.Current.GetOrSet(AvailableFeaturesCacheName, ctx =>
                GetAvailableModules()
                    .SelectMany(ext => ext.Features)
                    .OrderByDependenciesAndPriorities(HasDependency, GetPriority)
                    .ToArray(), TimeSpan.FromMinutes(10));
        }

        internal static int GetPriority(FeatureDescriptor featureDescriptor)
        {
            return featureDescriptor.Priority;
        }

        /// <summary>
        /// Returns true if the item has an explicit or implicit dependency on the subject
        /// </summary>
        /// <param name="item"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        internal static bool HasDependency(FeatureDescriptor item, FeatureDescriptor subject)
        {
            // Return based on explicit dependencies
            return item.Dependencies != null &&
                   item.Dependencies.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x, subject.Name));
        }
    }
}