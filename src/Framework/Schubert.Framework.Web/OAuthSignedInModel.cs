using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    /// <summary>
    /// 表示 OAuth 登录结果。
    /// </summary>
    public class OAuthSignedInModel
    {
        /// <summary>
        /// 获取或设置第三方提供程序的 Scheme。
        /// </summary>
        public String LoginProvider { get; set; }
        /// <summary>
        /// 第三方提供程序的显示名称。
        /// </summary>
        public String ProviderDisplayName { get; set; }
        /// <summary>
        /// 获取或设置第三方提供程序返回的 Key（对应当前的登录账号。）
        /// </summary>
        public String ProviderKey { get; set; }
        /// <summary>
        /// 获取或设置第三方账号的用户名 。
        /// </summary>
        public String ThirdPartUserName { get; set; }

        /// <summary>
        /// 获取或设置登陆成功后的返回地址。
        /// </summary>
        public String ReturnUrl { get; set; }
    }
}
