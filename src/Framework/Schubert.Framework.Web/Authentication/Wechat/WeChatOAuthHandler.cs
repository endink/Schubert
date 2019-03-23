using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Authentication.Wechat
{
    internal class WeChatOAuthHandler : OAuthHandler<WeChatOptions>
    {
        private const string _regexSEQuery = @"(?<=(\&|\?|^)({0})\=).*?(?=\&|$)";

        public WeChatOAuthHandler(IOptionsMonitor<WeChatOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        private async Task<String> RequestWechatOpenId(OAuthTokenResponse tokens)
        {
            string url = QueryHelpers.AddQueryString(this.Options.UserInformationEndpoint, "access_token ", tokens.AccessToken);
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            //
            //QQ 不支持 Bearer Token ，关于Bearer Token 更多信息，访问  http://blog.yorkxin.org/posts/2013/09/30/oauth2-6-bearer-token/
            //
            //httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            HttpResponseMessage httpResponseMessage = await this.Backchannel.SendAsync(httpRequestMessage, this.Context.RequestAborted);
            httpResponseMessage.EnsureSuccessStatusCode();

            //QQ 定义了错误返回格式，详情访问 :  http://wiki.open.qq.com/wiki/website/%E8%8E%B7%E5%8F%96%E7%94%A8%E6%88%B7OpenID_OAuth2.0
            string resultString = await httpResponseMessage.Content.ReadAsStringAsync();
            string errorCode = null;
            if (this.TryGetQueryValue(resultString, "code", out errorCode))
            {
                throw new InvalidOperationException($"Get QQ user openid error, error code :{errorCode} .");
            }
            //处理QQ获取OpenId 函数返回的特殊格式 callback( {"client_id":"YOUR_APPID","openid":"YOUR_OPENID"} )
            if (resultString.IndexOf("callback") != -1)
            {
                int lpos = resultString.IndexOf("(");
                int rpos = resultString.IndexOf(")");
                resultString = resultString.Substring(lpos + 1, rpos - lpos - 1);
            }

            JObject user = ParseResult(resultString);
            return user["openid"]?.ToString();
        }

        private async Task<JObject> RequestWechatUserInfo(string openId, OAuthTokenResponse tokens)
        {
            string url = "https://graph.qq.com/user/get_user_info";
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dictionary.Add("access_token", tokens.AccessToken);
            dictionary.Add("oauth_consumer_key", this.Options.ClientId);
            dictionary.Add("openid", openId);
            dictionary.Add("format", "json");
            url = QueryHelpers.AddQueryString(url, dictionary);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            HttpResponseMessage response = await this.Backchannel.SendAsync(request, this.Context.RequestAborted);
            response.EnsureSuccessStatusCode();

            string resultString = await response.Content.ReadAsStringAsync();
            JObject user = JObject.Parse(resultString);
            user["openid"] = openId;
            return user;
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            string openId = await RequestWechatOpenId(tokens);
            JObject user = await this.RequestWechatUserInfo(openId, tokens);
            
            if (!WeChatHelper.HasError(user))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, WeChatHelper.GetId(user), ClaimValueTypes.String));
                identity.AddClaim(new Claim("urn:qqaccount:openid", WeChatHelper.GetId(user), ClaimValueTypes.String));
                identity.AddClaim(new Claim(ClaimTypes.Name, WeChatHelper.GetNickName(user)));
                identity.AddClaim(new Claim(ClaimTypes.Gender, WeChatHelper.GetGender(user)));
                identity.AddClaim(new Claim(ClaimTypes.Email, $"{WeChatHelper.GetId(user)}@qq.com"));
            }

            OAuthCreatingTicketContext context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, this.Context, this.Scheme, this.Options, this.Backchannel, tokens, user);

            context.RunClaimActions();

            await Events.CreatingTicket(context);

            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }
        



        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            string defaultValue = this.FormatScope();
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dictionary.Add("response_type", "code");
            dictionary.Add("client_id", base.Options.ClientId);
            dictionary.Add("redirect_uri", redirectUri);
            WeChatOAuthHandler.AddQueryString(dictionary, properties, "scope", defaultValue);
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


        private bool TryGetQueryValue(string queryString, string key, out string value)
        {
            string pattern = String.Format(_regexSEQuery, key);
            bool success = Regex.IsMatch(queryString, pattern, RegexOptions.IgnoreCase);
            value = success ? Regex.Match(queryString, pattern).Value.Trim() : String.Empty;
            return success;

        }

        private JObject ParseResult(string resultString)
        {
            JObject user = null;

            JObject jResult = JObject.Parse(resultString);
            if (jResult.HasValues && jResult["openid"] != null && !String.IsNullOrWhiteSpace(jResult["openid"].ToString()))
            {
                user = jResult;
            }

            return user;
        }

    }
}