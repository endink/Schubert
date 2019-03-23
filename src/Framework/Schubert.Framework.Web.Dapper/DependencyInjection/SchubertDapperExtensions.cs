using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Web.AspNetIdentity;
using Schubert.Framework.Domain;
using Schubert.Framework.Web.DependencyInjection;
using Schubert.Framework.Web;
using Schubert.Framework.Services;

namespace Schubert
{
    public static class SchubertDapperExtensions
    {
        private static Guid _module = Guid.NewGuid();

        /// <summary>
        /// 加入 Asp.Net Identity ，同时使用 Dapper 持久化存储。
        /// </summary>
        /// <typeparam name="TUser">用户类型。</typeparam>
        /// <typeparam name="TRole">角色类型。</typeparam>
        /// <typeparam name="TIdentityService"><see cref="IIdentityService"/></typeparam>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static SchubertWebBuilder AddIdentityWithDapperStores<TUser, TRole, TIdentityService>(this SchubertWebBuilder builder, Action<IdentityOptions> configure = null)
            where TUser : UserBase
            where TRole : RoleBase
            where TIdentityService : IIdentityService
        {
            if (builder.AddedModules.Add(_module))
            {
                builder.AddStarter(new DapperIdentityStarter<TUser, TRole, TIdentityService>(configure));
            }
            return builder;
        }

        /// <summary>
        /// 为 Asp.Net Identity 配置 Dapper 持久化存储。
        /// </summary>
        /// <typeparam name="TUser">用户类型。</typeparam>
        /// <typeparam name="TRole">角色类型。</typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal static IdentityBuilder AddDapperStores<TUser, TRole>(this IdentityBuilder builder)
            where TUser : UserBase
            where TRole : RoleBase
        {
            builder.Services.AddSmart(ServiceDescriber.Scoped<IUserStore<TUser>, DapperUserStore<TUser, TRole>>(SmartOptions.Replace));
            builder.Services.AddSmart(ServiceDescriber.Scoped<IRoleStore<TRole>, DapperRoleStore<TRole>>(SmartOptions.Replace));

            return builder;

        }
    }
}
