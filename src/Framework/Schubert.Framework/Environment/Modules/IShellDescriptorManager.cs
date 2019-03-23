using System;
using System.Collections.Generic;

namespace Schubert.Framework.Environment.Modules
{
    /// <summary>
    /// Shell 信息存储服务。必须由开发人员自己实现。
    /// </summary>
    public interface IShellDescriptorManager
    {
        /// <summary>
        /// 返回 Shell 的配置信息。
        /// </summary>
        ShellDescriptor GetShellDescriptor();

        /// <summary>
        /// 更改存储介质中的Shell 配置信息。
        /// </summary>
        /// <param name="disabledFeatures">禁用的 Feautres 。</param>
        /// <param name="parameters"> Shell 参数。 </param>
        void UpdateShellDescriptor(
            IEnumerable<String> disabledFeatures,
            IEnumerable<ShellParameter> parameters);
    }
}