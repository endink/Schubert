using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment
{
    /// <summary>
    /// 工作上下文访问接口（通常此接口为 Singletone 模式）。
    /// </summary>
    public interface IWorkContextAccessor
    {
        /// <summary>
        /// 获取当前的工作上下文。
        /// </summary>
        /// <returns><see cref="WorkContext"/> 实例。</returns>
        WorkContext GetContext();
    }
}
