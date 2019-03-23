using System;
using System.Collections.Generic;

namespace Schubert.Framework.Environment.Modules.Finders
{
    /// <summary>
    /// 实现此接口以提供一种根据指定的位置或目录来组织模块的方法。
    /// 说明：位置或目录通常由 <see cref="EmbededFilesFinder"/>提供。
    /// <see cref="IModuleHarvester"/> 在 Schubert 框架中可以注册多个实例，
    /// 每一个<see cref="EmbededFilesFinder"/>将在同一位置对所有注册的 <see cref="IModuleHarvester"/>
    /// 调用 <see cref="TryHarvestModule(string, out ModuleDescriptor)"/> 方法，以实现多样化的模块组织方式。
    /// </summary>
    public interface IModuleHarvester
    {
        /// <summary>
        /// 指示提供的文件判断是否可以被加载。
        /// </summary>
        /// <param name="filePath">包含模块信息的文件名（例如 module.xml ）。</param>
        /// <returns>如果文件可以被加载，返回 true ， 否则，返回 false。</returns>
        bool CanHarvest(string filePath);
        /// <summary>
        /// 尝试在指定路径中组织模块。
        /// </summary>
        /// <param name="fileContent">包含模块信息的文件内容。</param>
        /// <param name="descriptor">获取到的模块信息。</param>
        /// <returns>如果模块信息获取成功，返回 true ， 否则，返回 false。</returns>
        bool TryHarvestModule(string fileContent, out ModuleDescriptor descriptor);
    }
}