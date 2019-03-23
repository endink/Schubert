using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Schubert.Framework.Web.Authentication.QQ
{
    public class QQOAuthOptions : OAuthOptions
    {
        public QQOAuthOptions()
        {
            //this.SignInScheme = QQDefaults.AuthenticationScheme;
            this.AuthorizationEndpoint = "https://graph.qq.com/oauth2.0/authorize";
            this.TokenEndpoint = "https://graph.qq.com/oauth2.0/token";
            this.UserInformationEndpoint = "https://graph.qq.com/oauth2.0/me";
            this.CallbackPath = new PathString(@"/signin-qq");
        }
    }
}
