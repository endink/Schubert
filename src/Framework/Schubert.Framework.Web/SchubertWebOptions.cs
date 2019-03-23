using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Schubert.Framework.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace Schubert.Framework.Web
{
    /// <summary>
    /// Schubert Web 特性选项（默认加载 Schubert : Web 配置节）。
    /// </summary>
    public class SchubertWebOptions
    {
        private int _sessionTimeoutMinutes;
        private JsonResolverSettings _jsonResolverSettings;

        public SchubertWebOptions()
        {
            this._sessionTimeoutMinutes = 30;
        }
        
        /// <summary>
        /// 是否使用基于 Cookie 的身份认证（默认为 true, 对于 web api 程序可以将其关闭）。
        /// </summary>
        public bool UseCookieAuth { get; set; } = true;

        /// <summary>
        /// 获取或设置一个值，指示包含的 ASP.Net MVC 特性（默认为 <see cref="MvcFeatures.Full"/>）。
        /// </summary>
        public MvcFeatures MvcFeatures { get; set; } = MvcFeatures.Full;

        /// <summary>
        /// 获取或设置一个值，指示框架是否包含 Session 功能，默认为 true。
        /// </summary>
        public bool UseSession { get; set; } = true;

        /// <summary>
        /// 获取或设置 Session 的超时时间（默认为30分钟, 小于 1 分钟视为 1 分钟）。
        /// </summary>
        public int SessionTimeoutMinutes
        {
            get { return this._sessionTimeoutMinutes; }
            set { this._sessionTimeoutMinutes = Math.Max(1, value); }
        }

        /// <summary>
        /// 获取或设置一值，指示在获取用户时候对获取操作使用缓存的过期时间（分钟），默认为10分钟。
        /// 重写 <see cref="Services.AbstractionWebIdentityService{TUser}.GetByIdAsync(long)"/> 方法时应考虑此参数。
        /// </summary>
        public int IdentityCacheTimeoutMinutes { get; set; } = 10;

        /// <summary>
        /// 获取或设置JSON的格式化的拼写风格（默认为 camel ）。
        /// </summary>
        public JsonCaseStyle JsonCaseStyle { get; set; } = JsonCaseStyle.CamelCase;

        /// <summary>
        /// 获取或设置 JSON 序列化时特殊处理模式 （通常用于解决 javascript 不支持 64 位整数问题）。
        /// </summary>
        public JsonResolverSettings JsonResolver
        {
            get { return _jsonResolverSettings ?? (_jsonResolverSettings = new JsonResolverSettings()); }
            set { _jsonResolverSettings = value; }
        }

        /// <summary>
        /// 获取或设置全局的路由路径前缀（例如 "api/v2"）。
        /// </summary>
        public string GlobalRoutePrefix { get; set; }
    }

    /// <summary>
    /// 表示 AspNetCore 的 Identity 特性可用性。
    /// </summary>
    [Flags]
    public enum IdentityUsage
    {
        /// <summary>
        ///  表示服务（如 UserManager, SiginManager 等）。
        /// </summary>
        Service = 1,
        /// <summary>
        /// 表示存储（使用 Entity Framework 默认存储）。
        /// </summary>
        EntityFrameworkStore = 3,
        /// <summary>
        /// 表示中间键（Cookie 身份验证等）。
        /// </summary>
        Middleware = 5,
    }
}
