using System;
using System.Reflection;

namespace Schubert.Framework.Environment.Modules.Loaders
{
    /// <summary>
    /// 实现此借口提供模块加载方法。
    /// <see cref="IModuleLoader"/> 在 Schubert 框架中可以注册多个，从而提供各种不同的加载方法。
    /// 对于一个<see cref="ModuleDescriptor"/> 描述的模块信息只要有一个加载器加载成功，后续的加载器将不再加载。
    /// </summary>
    public interface IModuleLoader
    {
        /// <summary>
        /// 从指定的指定模块信息中加载模块访问对象。
        /// </summary>
        /// <param name="descriptor">模块信息。</param>
        /// <param name="access">模块访问对象。</param>
        /// <returns>如果成功加载了模块访问对象，返回 true；否则，返回 false。</returns>
        bool TryLoad(ModuleDescriptor descriptor, out ModuleAccess access);
    }
}