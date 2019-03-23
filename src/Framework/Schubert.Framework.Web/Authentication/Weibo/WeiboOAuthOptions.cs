using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Schubert.Framework.Web.Authentication.Weibo
{
    public class WeiboOAuthOptions : OAuthOptions
    {
        public WeiboOAuthOptions()
        {
            //this.SignInScheme = WeiboDefaults.AuthenticationScheme;
            this.AuthorizationEndpoint = "https://api.weibo.com/oauth2/authorize";
            this.TokenEndpoint = "https://api.weibo.com/oauth2/access_token";
            this.UserInformationEndpoint = "https://api.weibo.com/oauth2/get_token_info";
            this.CallbackPath = new PathString(@"/signin-weibo");
        }
    }
}
