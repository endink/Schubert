using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Schubert.Framework.Web.Authentication.Wechat
{
    public class WeChatOptions : OAuthOptions
    {
        public WeChatOptions()
        {
            //this.SignInScheme = WechatDefaults.AuthenticationScheme;
            this.AuthorizationEndpoint = "https://open.weixin.qq.com/connect/qrconnect";
            this.TokenEndpoint = "https://api.weixin.qq.com/sns/oauth2/access_token";
            this.UserInformationEndpoint = "https://graph.qq.com/me";
            
            this.CallbackPath = new PathString(@"/signin-weixin");
        }
    }
}
