using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    partial class ExtensionMethods
    {
        /// <summary>
        /// 判断请求是否是 ajax 请求（支持 jquery）。
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>此方法使用 X-Requested-With 消息头是否具有值 XMLHttpRequest 为判定依据，不保证客户端兼容，受 jquery 支持。</remarks>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        public static bool IsForwardedSsl(this HttpRequest request)
        {
            return request.IsProtoForwardedSsl() || request.IsAzureForwardedSsl();
        }


        private static bool IsProtoForwardedSsl(this HttpRequest request)
        {
            var xForwardedProto = request.Headers.FirstOrDefault(x => x.Key.CaseInsensitiveEquals("X-Forwarded-Proto"));
            var forwardedSsl = xForwardedProto.Value.Any() &&
              xForwardedProto.Value.Any(x => x.CaseInsensitiveEquals("https"));
            return forwardedSsl;
        }

        private static bool IsAzureForwardedSsl(this HttpRequest request)
        {
            var xForwardedProto = request.Headers.FirstOrDefault(x => x.Key.CaseInsensitiveEquals("X-ARR-SSL"));
            var forwardedSsl = xForwardedProto.Value.Any();
            return forwardedSsl;
        }
    }
}
