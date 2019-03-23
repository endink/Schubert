using System;
using System.Collections.Generic;

namespace Schubert.Framework.Environment.Modules.Finders
{
    /// <summary>
    /// 实现模块定位器接口。
    /// <see cref="IModuleFinder"/> 在 Schubert 框架中可以注册多个，从而以不同的方式定位模块。
    /// 在加载模块时所有的定位器 <see cref="FindAvailableModules"/> 方法都会被调用，每一个定位器都将尝试以自己的方式定位模块。
    /// </summary>
    public interface IModuleFinder
    {
        /// <summary>
        /// 实现此方法，用于定位应用程序模块。
        /// </summary>
        /// <returns></returns>
        IEnumerable<ModuleDescriptor> FindAvailableModules();
    }
}