using Schubert.Framework.Environment.Modules;
using System;
using System.Collections.Generic;
using Schubert.Framework.Events;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Schubert.Framework.Environment.ShellBuilders
{
    /// <summary>
    /// 描述应用程序蓝图。
    /// </summary>
    public class ShellBlueprint
    {
        /// <summary>
        /// 获取或设置蓝图中的 feature 。
        /// </summary>
        public string[] Features { get; internal set; }

        /// <summary>
        /// 获取或设置蓝图中的 module。
        /// </summary>
        public string[] Modules { get; internal set; }

        /// <summary>
        /// 获取或设置 Shell 描述信息。
        /// </summary>
        public ShellDescriptor Descriptor { get; internal set; }

        /// <summary>
        /// 获取或设置蓝图中的 <see cref="IDependency"/>（用于 DI 进行依赖注入）。
        /// </summary>
        public IEnumerable<ShellBlueprintItem> Dependencies => (this.ExportedItems?.GetOrDefault(BuiltinBlueprintItemCategories.Dependency) ?? Enumerable.Empty<ShellBlueprintItem>());

        /// <summary>
        /// 获取或设置蓝图中的 Controller（用于 MVC 呈现界面）。
        /// </summary>
        public IEnumerable<ControllerBlueprintItem> Controllers => (this.ExportedItems?.GetOrDefault(BuiltinBlueprintItemCategories.Controller) ?? Enumerable.Empty<ShellBlueprintItem>()).OfType<ControllerBlueprintItem>();

        /// <summary>
        /// 获取或设置蓝图中的 <see cref="IDependencyDescriber"/> 。
        /// </summary>
        public IEnumerable<ShellBlueprintItem> DependencyDescribers  => (this.ExportedItems?.GetOrDefault(BuiltinBlueprintItemCategories.DependencyDescriber) ?? Enumerable.Empty<ShellBlueprintItem>()).OfType<ShellBlueprintItem>();

        /// <summary>
        /// 获取或设置配置选项。
        /// </summary>
        public IEnumerable<OptionsBlueprintItem> ConfiguredOptions => (this.ExportedItems?.GetOrDefault(BuiltinBlueprintItemCategories.Options) ?? Enumerable.Empty<ShellBlueprintItem>()).OfType<OptionsBlueprintItem>();

        /// <summary>
        /// 获取或设置自定义蓝图项。
        /// </summary>
        public IDictionary<String, IEnumerable<ShellBlueprintItem>> ExportedItems { get; set; } = new Dictionary<String, IEnumerable<ShellBlueprintItem>>(0);
    }

    /// <summary>
    /// 应用程序蓝图组件。
    /// </summary>
    public class ShellBlueprintItem
    {
        public Type Type { get; set; }
        public Feature Feature { get; set; }

    }

    public class ShellBlueprintDependencyItem : ShellBlueprintItem
    {
        public HashSet<Tuple<Type, ServiceLifetime>> Interfaces { get; } = new HashSet<Tuple<Type, ServiceLifetime>>();
    }

    /// <summary>
    /// 表示一个通过配置加载的选项。
    /// </summary>
    public class OptionsBlueprintItem : ShellBlueprintItem
    {
        /// <summary>
        /// 获取或设置配置节名称。
        /// </summary>
        public string ConfigurationSection { get; set; }
    }

    /// <summary>
    /// 表示一个 MVC Controller 组件。
    /// </summary>
    public class ControllerBlueprintItem : ShellBlueprintItem
    {
        /// <summary>
        /// 获取或设置 Controller 名称。
        /// </summary>
        public string ControllerName { get; set; }
    }
}