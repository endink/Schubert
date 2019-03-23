using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment
{
    /// <summary>
    /// 应用程序实例 ID 提供程序（用于区分同一应用的多个实例）。
    /// </summary>
    public interface IInstanceIdProvider
    {
        /// <summary>
        /// 获取当前实例的 Id。
        /// </summary>
        /// <returns></returns>
        String GetInstanceId();
    }
}
