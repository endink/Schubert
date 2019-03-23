using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    /// <summary>
    /// 表示 MVC 功能特性的枚举。
    /// </summary>
    public enum MvcFeatures
    {
        /// <summary>
        /// 表示不包含任何 MVC 功能。
        /// </summary>
        None,
        /// <summary>
        /// 表示 MVC 完整功能。
        /// </summary>
        Full,
        /// <summary>
        /// 表示仅使用 MVC Web Api 的核心功能。
        /// </summary>
        Api,
        /// <summary>
        /// 表示基础功能（参考：https://github.com/aspnet/Announcements/issues/49）
        /// </summary>
        Core,
    }
}
