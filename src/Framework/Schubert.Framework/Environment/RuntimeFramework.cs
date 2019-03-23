using System;

namespace Schubert.Framework.Environment
{
    /// <summary>
    /// 表示 Asp.Net 使用的 Framework 平台的枚举。
    /// </summary>
    public enum RuntimeFramework
    {
        /// <summary>
        /// 未知平台（或许是 Mono ?）。
        /// </summary>
        Unknown,
        /// <summary>
        /// 表示 .Net Core Framework 。
        /// </summary>
        DNXCore,
        /// <summary>
        /// 表示 DNX Framwork。
        /// </summary>
        DNX,
        /// <summary>
        /// 表示.Net Framework
        /// </summary>
        Net
    }
}