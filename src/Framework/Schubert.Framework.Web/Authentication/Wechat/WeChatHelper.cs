using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Schubert.Framework.Web.Authentication.Wechat
{
    /// <summary>
    /// //QQ API 返回参数参考 http://wiki.open.qq.com/wiki/website/get_user_info#3.2.E8.BE.93.E5.85.A5.E5.8F.82.E6.95.B0.E8.AF.B4.E6.98.8E
    /// </summary>
    public static class WeChatHelper 
    {
        public static string GetId(JObject user)
        {
            return user.Value<String>("openid");
        }
        /// <summary>
        /// 获取是否出错， ret为返回状态码，参见http://wiki.open.qq.com/wiki/website/%E5%85%AC%E5%85%B1%E8%BF%94%E5%9B%9E%E7%A0%81%E8%AF%B4%E6%98%8E
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool HasError(JObject user)
        {
            return user.Value<int>("ret") != 0;
        }

        public static string GetErrorMessage(JObject user)
        {
            return user.GetValueOrDefault("msg");
        }

        public static string GetNickName(JObject user)
        {
            return user.GetValueOrDefault("nickname");
        }

        /// <summary>
        /// 40*40 用户头像（图片地址）。
        /// </summary>
        public static string GetAvatarUrl(JObject user)
        {
            return user.GetValueOrDefault("figureurl_qq_1");
        }

        public static string GetGender(JObject user)
        {
            return user.GetValueOrDefault("gender");
        }

        /// <summary>
        /// 100*100 用户头像（图片地址，注意，100*100头像不是所有用户都会设置，40*40所有用户都有）。
        /// </summary>
        public static string GetLargeAvatarUrl(JObject user)
        {
            return user.GetValueOrDefault("figureurl_qq_2");
        }
    }
}