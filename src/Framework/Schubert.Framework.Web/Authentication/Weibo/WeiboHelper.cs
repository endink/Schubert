using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Schubert.Framework.Web.Authentication.Weibo
{
    /// <summary>
    /// Weibo API 返回参数参考 http://open.weibo.com/wiki/Oauth2/get_token_info
    /// </summary>
    public static class WeiboHelper
    {
        public static string GetId(JObject user)
        {
            return user.Value<String>("uid");
        }
    }
}