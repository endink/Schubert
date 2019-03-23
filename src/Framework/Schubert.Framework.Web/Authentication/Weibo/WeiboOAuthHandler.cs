using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Schubert.Framework.Web.Authentication.Weibo
{
    internal class WeiboOAuthHandler : OAuthHandler<WeiboOAuthOptions>
    {
        public WeiboOAuthHandler(IOptionsMonitor<WeiboOAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        private async Task<JObject> RequestWeiboUidAsync(OAuthTokenResponse tokens)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, this.Options.UserInformationEndpoint);
            FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>
            {

                {
                    "access_token",
                    tokens.AccessToken
                }
            });
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequestMessage.Content = content;
            HttpResponseMessage httpResponseMessage = await this.Backchannel.SendAsync(httpRequestMessage, this.Context.RequestAborted);
            httpResponseMessage.EnsureSuccessStatusCode();
            
            string resultString = await httpResponseMessage.Content.ReadAsStringAsync();
            JObject user = JObject.Parse(resultString);

            //微博API统一错误处理 http://open.weibo.com/wiki/%E6%8E%88%E6%9D%83%E6%9C%BA%E5%88%B6%E8%AF%B4%E6%98%8E
            if (user["error"] != null)
            {
                throw new InvalidOperationException($"Get weibo token info error,  {user["error_description"]}. ( error code : {user["error_code"]} )");
            }

            return user;
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            JObject user = await this.RequestWeiboUidAsync(tokens);


            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, WeiboHelper.GetId(user), ClaimValueTypes.String));

            OAuthCreatingTicketContext context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity),
                properties, this.Context, this.Scheme, this.Options, this.Backchannel, tokens, user);
            
            await this.Options.Events.CreatingTicket(context);
            context.RunClaimActions();

            await Events.CreatingTicket(context);

            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }



        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            string defaultValue = this.FormatScope();
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            //dictionary.Add("response_type", "code");
            dictionary.Add("client_id", base.Options.ClientId);
            dictionary.Add("redirect_uri", redirectUri);
            dictionary.Add("forcelogin", "true");
            WeiboOAuthHandler.AddQueryString(dictionary, properties, "scope", defaultValue);
            string text = base.Options.StateDataFormat.Protect(properties);
            dictionary.Add("state", text);
            return QueryHelpers.AddQueryString(base.Options.AuthorizationEndpoint, dictionary);
        }
        protected override string FormatScope()
        {
            return String.Join(",", this.Options.Scope);
        }

        private static void AddQueryString(IDictionary<string, string> queryStrings, AuthenticationProperties properties, string name, string defaultValue = null)
        {
            string text;
            if (!properties.Items.TryGetValue(name, out text))
            {
                text = defaultValue;
            }
            else
            {
                properties.Items.Remove(name);
            }
            if (text == null)
            {
                return;
            }
            queryStrings[name] = text;
        }


    }
}