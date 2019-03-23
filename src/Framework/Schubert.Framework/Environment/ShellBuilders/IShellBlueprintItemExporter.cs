using Schubert.Framework.Environment.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment.ShellBuilders
{
    /// <summary>
    /// 自定义的 <see cref="ShellBlueprintItem"/> 导出器。
    /// </summary>
    public interface IShellBlueprintItemExporter
    {
        /// <summary>
        /// 选择器创建 item 类型
        /// </summary>
        string Category { get; }

        /// <summary>
        /// 决定一个类型是否能够被导出。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool CanExport(Type type);

        /// <summary>
        /// 导出一个 <see cref="ShellBlueprintItem"/>。
        /// </summary>
        /// <param name="type">要导出的类型。</param>
        /// <param name="feature">类型所在的 <see cref="Feature"/>。</param>
        /// <returns></returns>
        ShellBlueprintItem Export(Type type, Feature feature);
    }

    public class BuiltinBlueprintItemCategories
    {
        public const string Controller = "Controller";
        public const string DependencyDescriber = "DependencyDescriber";
        public const string Dependency = "Dependency";
        public const string Options = "Options";
    }
}
