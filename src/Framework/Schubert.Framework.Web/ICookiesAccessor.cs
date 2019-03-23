using Microsoft.AspNetCore.Http;
using System;

namespace Schubert.Framework.Web
{
    public interface ICookiesAccessor
    {
        string ReadCookie(string key);

        /// <summary>
        /// 以 <paramref name="options"/> 设置的选项，存储键和值到 Cookie。
        /// </summary>
        /// <param name="key">要存储的 Cookie 键。</param>
        /// <param name="value">要存储的 Cookie 值。</param>
        /// <param name="options">Cookie 存储选项。</param>
        void SaveCookie(string key, string value, CookieOptions options);

        /// <summary>
        /// 以 <paramref name="cookieOptions"/> 设置的选项（ Path 等属性影响选择器），删除指定键的 Cookie。
        /// </summary>
        /// <param name="key">要删除的 Cookie 键。</param>
        /// <param name="cookieOptions">Cookie 存储选项。</param>
        void DeleteCookie(string key, CookieOptions cookieOptions);
    }

    public static class ICookiesAccessorExtensions
    {
        /// <summary>
        /// 以 <paramref name="expires"/> 设置的过期时间，存储键和值到 Cookie。
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="key">要存储的 Cookie 键。</param>
        /// <param name="value">要存储的 Cookie 值。</param>
        /// <param name="expires">过期时间，为 null 表示不过期。</param>
        public static void SaveCookie(this ICookiesAccessor accessor, string key, string value, DateTime? expires)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = expires;
            accessor.SaveCookie(key, value, options);

        }

        /// <summary>
        /// 存储键和值到 Cookie。
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="key">要存储的 Cookie 键。</param>
        /// <param name="value">要存储的 Cookie 值。</param>
        public static void SaveCookie(this ICookiesAccessor accessor, string key, string value)
        {
            accessor.SaveCookie(key, value, (CookieOptions)null);

        }

        /// <summary>
        /// 删除指定键的 Cookie。
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="key">要删除的 Cookie 键。</param>
        public static void DeleteCookie(this ICookiesAccessor accessor, string key)
        {
            accessor.DeleteCookie(key, new CookieOptions());
        }
        
    }
}