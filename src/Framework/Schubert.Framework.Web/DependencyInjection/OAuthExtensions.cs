using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.Web.Authentication.QQ;
using Schubert.Framework.Web.Authentication.Wechat;
using Schubert.Framework.Web.Authentication.Weibo;
using Schubert.Framework.Web.DependencyInjection;
using System;
using System.ComponentModel;

namespace Schubert
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OAuthExtensions
    {
        /// <summary>
        /// 使用QQ登陆。
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static SchubertWebBuilder AddQQSignIn(this SchubertWebBuilder app, Action<QQOAuthOptions> configureOptions = null)
        {
            QQOAuthOptions options = new QQOAuthOptions();
            configureOptions?.Invoke(options);
            return app.ConfigureServices(s => 
            s.AddAuthentication().AddOAuth<QQOAuthOptions, QQOAuthHandler>(QQDefaults.AuthenticationScheme, QQDefaults.DisplayName, configureOptions));
        }

        /// <summary>
        /// 使用微博登陆。
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static SchubertWebBuilder AddWeiboSignIn(this SchubertWebBuilder app, Action<WeiboOAuthOptions> configureOptions = null)
        {
            WeiboOAuthOptions options = new WeiboOAuthOptions();
            configureOptions?.Invoke(options);
            return  app.ConfigureServices(s => s.AddAuthentication().AddOAuth<WeiboOAuthOptions, WeiboOAuthHandler>(WeiboDefaults.AuthenticationScheme, WeiboDefaults.DisplayName, configureOptions));
        }

        /// <summary>
        /// 使用微信登陆。
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static SchubertWebBuilder AddWeChatSignIn(this SchubertWebBuilder app, Action<WeChatOptions> configureOptions = null)
        {
            WeChatOptions options = new WeChatOptions();
            configureOptions?.Invoke(options);
            return app.ConfigureServices(s => s.AddAuthentication().AddOAuth<WeChatOptions, WeChatOAuthHandler>(WeiboDefaults.AuthenticationScheme, WechatDefaults.DisplayName, configureOptions));
        }
    }
}