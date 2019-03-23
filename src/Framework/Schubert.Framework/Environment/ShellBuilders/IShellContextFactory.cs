using Microsoft.Extensions.Configuration;
using System;

namespace Schubert.Framework.Environment.ShellBuilders
{
    /// <summary>
    /// 实现此接口用以创建 <see cref="ShellContext"/> 对象。
    /// </summary>
    public interface IShellContextFactory
    {
        /// <summary>
        /// 创建 Shell 运行时上下文。
        /// </summary>
        /// <returns>创建的 <see cref="ShellContext"/> 实例。</returns>
        ShellContext CreateShellContext();

        /// <summary>
        /// 创建初次使用安装环境的运行时上下文。
        /// </summary>
        /// <returns>创建的 <see cref="ShellContext"/> 实例。</returns>
        ShellContext CreateSetupContext();
    }
}