using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Schubert.Framework.Web.DependencyInjection;
using Schubert.Framework.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;

namespace Schubert
{
    public static partial class SchubertWebUtility
    {
        /// <summary>
        /// 获取 Windows 用户文件夹。
        /// </summary>
        /// <returns></returns>
        public static string GetUserDirectory()
        {
            return System.Environment.GetEnvironmentVariable("USERPROFILE");
        }

        public static SchubertWebBuilder AddWebApiConventions(this SchubertWebBuilder builder)
        {
            //builder.Services.TryAddEnumerable(
            //    ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, WebApiCompatShimOptionsSetup>());
            //builder.Services.TryAddEnumerable(
            //    ServiceDescriptor.Transient<IConfigureOptions<WebApiCompatShimOptions>, WebApiCompatShimOptionsSetup>());

            //builder.Services.TryAddSingleton<IContentNegotiator, DefaultContentNegotiator>();

            //return builder;

            builder.ConfigureServices(services =>
                services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, ApiOptionsSetup>()));

            return builder;
        }
        

        public static string HtmlEncode(string input)
        {
            return HtmlEncoder.Default.Encode(input);
        }

        public static string UrlEncode(string input)
        {
            return UrlEncoder.Default.Encode(input);
        }

        public static string JavaScriptEncode(string input)
        {
            return JavaScriptEncoder.Default.Encode(input); 
        }

        public static string Base64UrlEncode(byte[] input)
        {
            return WebEncoders.Base64UrlEncode(input);
        }

        public static byte[] Base64UrlDecode(string input)
        {
            return WebEncoders.Base64UrlDecode(input);
        }

        public static IDictionary<string, string> ReadFormString(string queryString, bool unescapeDataString = true)
        {
            
            using (FormReader reader = new FormReader(queryString))
            {
               var collection = new FormCollection(reader.ReadForm());
                Dictionary<string, string> result = new Dictionary<string, string>(collection.Count);
                foreach (var kv in collection)
                {
                    result[kv.Key] = Uri.UnescapeDataString(kv.Value);
                }
                return result;
            }
        }
    }
}
