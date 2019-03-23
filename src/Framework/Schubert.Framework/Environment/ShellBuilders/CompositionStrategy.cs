using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Schubert.Framework.Environment.Modules;
using Schubert.Framework.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Schubert.Framework.Environment.ShellBuilders
{
    /// <summary>
    ///  <see cref="ICompositionStrategy"/> 接口的默认实现。
    /// </summary>
    public class CompositionStrategy : ICompositionStrategy
    {
        internal const string FrameworkFeatureName = "Schubert.Framework";
        private readonly IFeatureManager _featureManager;
        private ILogger _logger;
        private StringBuilder _verboseBuilder;
        private string _appName = null;
        private IEnumerable<IShellBlueprintItemExporter> _exporters;

        /// <summary>
        /// 创建 <see cref="CompositionStrategy"/> 的新实例。
        /// </summary>
        /// <param name="featureManager"><see cref="IFeatureManager"/> 对象。</param>
        /// <param name="loggerFactory">日志工厂对象，提供 <see cref="ILogger"/> 用于记录创建蓝图过程中的必要步骤。</param>
        /// <param name="options">Schubert 框架基础配置。</param>
        /// <param name="exporters">Shell 部件导出器。</param>
        public CompositionStrategy(IFeatureManager featureManager, ILoggerFactory loggerFactory, IOptions<SchubertOptions> options, IEnumerable<IShellBlueprintItemExporter> exporters)
        {
            Guard.ArgumentNotNull(featureManager, nameof(featureManager));

            _exporters = exporters ?? exporters;
            _appName = options.Value.AppSystemName;
            _featureManager = featureManager;
            _logger = loggerFactory?.CreateLogger<CompositionStrategy>() ?? (ILogger)NullLogger.Instance;
        }

        /// <summary>
        /// 参见 <see cref="ICompositionStrategy.Compose(string, ShellDescriptor)"/> 方法。
        /// </summary>
        public ShellBlueprint Compose(string applicationName, ShellDescriptor descriptor)
        {
            Guard.ArgumentNotNull(descriptor, nameof(descriptor));
            _logger.WriteDebug("应用组合策略。");
            IEnumerable<Feature> features = _featureManager.GetEnabledFeatures().ToArray();
            _logger.WriteDebug($"应用组合策略， Feature 数量 {features.Count()}");

            //为什么不用 Union? Union 对于引用类型来说和 Concat 无差别，性能反而更差。
            features = features.Concat(BuiltinFeatures()).Distinct(f => f.Descriptor.Name);

            var excludedTypes = GetExcludedTypes(features);

            return BuildBlueprint(descriptor, features, excludedTypes);
        }

        /// <summary>
        /// 创建 Schubert 框架内置的组件。
        /// </summary>
        private static IEnumerable<Feature> BuiltinFeatures()
        {
            yield return new Feature
            {
                
                Descriptor = new FeatureDescriptor
                {
                    Name = FrameworkFeatureName,
                    Category = "Builtin",
                    ModuleName = FrameworkFeatureName,
                    Priority = 999
                },
                ExportedTypes = Enumerable.Empty<Type>()
            };
        }

        private void BeginTrace()
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                if (_verboseBuilder != null)
                {
                    _verboseBuilder = null;
                }
                _verboseBuilder = new StringBuilder();
            }
        }

        private void AddTraceLine(string message)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                if (_verboseBuilder == null)
                {
                    _verboseBuilder = new StringBuilder();
                }
                _verboseBuilder.AppendLine(message);
            }
        }

        private void EndTrace()
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                if (_verboseBuilder != null)
                {
                    string text = _verboseBuilder.ToString();
                    if (!text.IsNullOrWhiteSpace())
                    {
                        _logger.WriteTrace(text);
                    }
                    _verboseBuilder = null;
                }
            }
        }

        private ShellBlueprint BuildBlueprint(ShellDescriptor descriptor, IEnumerable<Feature> features, IEnumerable<String> excludeTypes)
        {
            _logger.WriteInformation($"开始加载蓝图（app: {_appName}）。");
            string[] feautureNames = features.Select(f => f.Descriptor.Name).ToArray();
            string[] modules = features.Select(f => f.Descriptor.ModuleName).Distinct().ToArray();

            var items = new Dictionary<String, List<ShellBlueprintItem>>();

            ShellBlueprint blueprint = new ShellBlueprint() { Features = feautureNames, Modules = modules };
            
            foreach (var f in features)
            {
                if (f.Descriptor.ModuleName.Equals(FrameworkFeatureName))
                {
                    continue;
                }
                if (!f.ExportedTypes.Any())
                {
                    this._logger.WriteTrace($"跳过 Feature ( Name={f.Descriptor.Name}, Module={f.Descriptor.ModuleName} )，由于没有导出类型。");
                    continue;
                }

                this.BeginTrace();
                this.AddTraceLine($"加载 Feature ( Name={f.Descriptor.Name}, Module={f.Descriptor.ModuleName} )");
                foreach (Type t in f.ExportedTypes)
                {
                    if (!excludeTypes.Contains(t.FullName))
                    {
                        foreach (var e in _exporters)
                        {
                            if (e.CanExport(t))
                            {
                                var item = e.Export(t, f);
                                if (item != null)
                                {
                                    var list = items.GetOrAdd(e.Category, c=> new List<ShellBlueprintItem>());
                                    list.Add(item);
                                    this.AddTraceLine($"发现 {e.Category}：{t.Name}。");
                                }
                            }
                        }
                    }
                }

                this.EndTrace();
            }

            var saveItems = new Dictionary<String, IEnumerable<ShellBlueprintItem>>(items.Count);
            foreach (var kv in items)
            {
                saveItems.Add(kv.Key, kv.Value.ToArray());
            }

            
            //设置蓝图。
            blueprint.Descriptor = descriptor;
            blueprint.ExportedItems = saveItems;

            
            _logger.WriteInformation($"蓝图加载完成。");

            return blueprint;
        }

        

        /// <summary>
        /// 获取被 SuppressDependencyAttribute 标记过的要进行替换的类型名称。
        /// </summary>
        private IEnumerable<string> GetExcludedTypes(IEnumerable<Feature> features)
        {
            var excludedTypes = new HashSet<string>();
            
            foreach (Feature feature in features)
            {
                this.BeginTrace();

                foreach (Type type in feature.ExportedTypes)
                {
                    var attributes = type.GetTypeInfo().GetCustomAttributes(typeof(SuppressDependencyAttribute), false);
                    foreach (SuppressDependencyAttribute replacedType in attributes)
                    {
                        excludedTypes.Add(replacedType.TypeFullName);
                        this.AddTraceLine($"Feature ({feature.Descriptor.Name}) 中发现覆盖类型：{replacedType.TypeFullName} 被 {type.FullName} 覆盖。");
                    }
                }

                this.EndTrace();
            }

            return excludedTypes;
        }
    }
}