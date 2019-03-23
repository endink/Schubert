using Schubert.Framework.Environment.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Schubert.Framework.Environment.Modules
{
    /// <summary>
    /// 模块管理接口（用于 Hosting 层面，开发人员无需关心）。
    /// </summary>
    public interface IModuleManager
    {
        /// <summary>
        /// 获取应用程序中所有可用的模块。
        /// </summary>
        /// <returns></returns>
        IEnumerable<ModuleDescriptor> GetAvailableModules();

        /// <summary>
        /// 获取应用程序中所有的 Features （从每个模块中加载 Features）。
        /// </summary>
        /// <returns></returns>
        IEnumerable<FeatureDescriptor> GetAvailableFeatures();

        ModuleDescriptor GetModule(string name);
    }

    public static class ModuleManagerExtensions
    {
        public static IEnumerable<FeatureDescriptor> GetEnabledFeatures(this IModuleManager m, ShellDescriptor descriptor)
        {
            return m.GetAvailableFeatures().Where(fd => !descriptor.DisabledFeatures.Any(sf => sf == fd.Name));
        }
    }
}