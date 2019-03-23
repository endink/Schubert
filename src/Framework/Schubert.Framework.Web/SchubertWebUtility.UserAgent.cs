using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Schubert.Framework.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Schubert
{
    partial class SchubertWebUtility
    {
        public static String GetUserAgent(this HttpRequest request)
        {
            if (request.Headers.TryGetValue(HeaderNames.UserAgent, out StringValues userAgent))
            {
                return StringValues.IsNullOrEmpty(userAgent) ? null : userAgent.ToString();
            }
            return String.Empty;
        }

        private static bool IsUserAgentMatched(this HttpRequest request, Func<String, bool> precidate)
        {
            StringValues userAgent;
            if (request.Headers.TryGetValue(HeaderNames.UserAgent, out userAgent) && !StringValues.IsNullOrEmpty(userAgent))
            {
                string agentString = userAgent.ToString();
                return precidate?.Invoke(agentString) ?? false;
            }
            return false;
        }

        public static bool IsWindows(this HttpRequest request)
        {
            return request.IsUserAgentMatched(s => UserAgent.IsWindows(s));
        }

        public static bool IsWindowsTablet(this HttpRequest request)
        {
            return request.IsUserAgentMatched(s => UserAgent.IsWindowsTablet(s));
        }

        public static bool IsWindowsPhone(this HttpRequest request)
        {
            return request.IsUserAgentMatched(s => UserAgent.IsWindowsPhone(s));
        }

        public static bool IsMacOS(this HttpRequest request)
        {
            return request.IsUserAgentMatched(s => UserAgent.IsMacOS(s));
        }

        public static bool IsIPhone(this HttpRequest request)
        {
            return request.IsUserAgentMatched(s => UserAgent.IsIPhone(s));
        }

        public static bool IsIPad(this HttpRequest request)
        {
            return request.IsUserAgentMatched(s => UserAgent.IsIPad(s));
        }

        public static bool IsAndroid(this HttpRequest request)
        {
            return request.IsUserAgentMatched(s => UserAgent.IsAndroid(s));
        }

        public static bool IsAndroidPhone(this HttpRequest request)
        {
            return request.IsUserAgentMatched(s => UserAgent.IsAndroidPhone(s));
        }

        public static bool IsAndroidTablet(this HttpRequest request)
        {
            return request.IsUserAgentMatched(s => UserAgent.IsAndroidTablet(s));
        }

        /// <summary>
        /// 判断当前客户端使用的浏览器是否为 Microsoft IE。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="includeIEMobile">是否将 IE 移动版视为IE。</param>
        /// <returns></returns>
        private static bool IsMicosoftIE(this HttpRequest request, bool includeIEMobile = false)
        {
            int v;
            return request.TryParseMicosoftIE(includeIEMobile, out v);
        }


        /// <summary>
        /// 尝试获取客户端IE版本，如果不是IE浏览器将返回 -1，否则返回浏览器版本号。
        /// 如之想知道是否使用IE, 可以使用 <see cref="IsMicosoftIE(HttpRequest, bool)"/> 方法。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="includeIEMobile">是否将 IE 移动版视为IE。</param>
        /// <returns></returns>
        public static int GetMicosoftIEVersion(this HttpRequest request, bool includeIEMobile = false)
        {
            int v;
            request.TryParseMicosoftIE(includeIEMobile, out v);
            return v;
        }

        private static bool TryParseMicosoftIE(this HttpRequest request, bool includeIEMobile, out int version)
        {
            version = -1;
            StringValues userAgent;
            if (request.Headers.TryGetValue(HeaderNames.UserAgent, out userAgent) && !StringValues.IsNullOrEmpty(userAgent))
            {
                string agentString = userAgent.ToString();
                if (!agentString.IsNullOrWhiteSpace())
                {
                    Match match = Regex.Match(agentString, @"(msie) ([\w.]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        version = (int)float.Parse(match.Groups[2].Value);
                        return true;
                    }

                    match = agentString.IndexOf("compatible") < 0 ?  Regex.Match(agentString, @"(mozilla)(?:.*? rv:([\w.]+)|)", RegexOptions.IgnoreCase) : Match.Empty;
                    if (match.Success)
                    {
                        version = (int)float.Parse(match.Groups[2].Value);
                        return true;
                    }

                    match = agentString.IndexOf("trident") >= 0 ? Regex.Match(agentString, @"(rv)(?::| )([\w.]+)", RegexOptions.IgnoreCase) : Match.Empty;
                    if (match.Success)
                    {
                        version = (int)float.Parse(match.Groups[2].Value);
                        return true;
                    }

                    match = includeIEMobile ? Regex.Match(agentString, @"(iemobile)[\/]([\w.]+)", RegexOptions.IgnoreCase) : Match.Empty;
                    if (match.Success)
                    {
                        version = (int)float.Parse(match.Groups[2].Value);
                        return true;
                    }

                }
            }
            return false;
        }
    }
}
