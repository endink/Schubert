using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    /// <summary>
    /// 表示一个第三方登陆错误。
    /// </summary>
    public enum OAuthSignInError
    {
        /// <summary>
        /// 表示一个非法的请求（通常表示第三方 OAuth 登录会话错误）。
        /// </summary>
        InvalidRequest,
        /// <summary>
        /// 表示用户被锁定。
        /// </summary>
        UserLockedOut,
        /// <summary>
        /// 表示禁止访问（未授权）。
        /// </summary>
        NotAllowed
    }
}
