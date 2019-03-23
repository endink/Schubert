using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace Schubert.Framework.Web
{
    public class CookiesAccessor : ICookiesAccessor
    {
        private IHttpContextAccessor _httpContextAccessor;

        public CookiesAccessor(IHttpContextAccessor httpContextAccessor)
        {
            Guard.ArgumentNotNull(httpContextAccessor, nameof(httpContextAccessor));

            _httpContextAccessor = httpContextAccessor;
        }

        private void ThrowIfNotExistContext()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new SchubertException("当前上下文中不包含 HttpContext， 可能不是一个 Web 请求，单元测试请进行 mock。");
            }
        }


        /// <summary>
        /// 读取指定键的 Cookie 值。
        /// </summary>
        /// <param name="key">Cookie 键。</param>
        /// <returns>返回 Cookie 的值，如果未找到 Cookie，返回 null。</returns>
        public string ReadCookie(string key)
        {
            ThrowIfNotExistContext();
            if (_httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(key))
            {
                return _httpContextAccessor.HttpContext.Request.Cookies[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 以 <paramref name="options"/> 设置的选项，存储键和值到 Cookie。
        /// </summary>
        /// <param name="key">要存储的 Cookie 键。</param>
        /// <param name="value">要存储的 Cookie 值。</param>
        /// <param name="options">保存选项。</param>
        public void SaveCookie(string key, string value, CookieOptions options)
        {
            ThrowIfNotExistContext();

            options = options ?? new CookieOptions();
            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, options);
        }

        public void DeleteCookie(string key, CookieOptions cookieOptions)
        {
            ThrowIfNotExistContext();
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(key, cookieOptions);
        }
    }
}
