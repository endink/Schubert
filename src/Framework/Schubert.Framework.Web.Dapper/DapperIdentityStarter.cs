using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Domain;
using Schubert.Framework.Services;
using System;

namespace Schubert.Framework.Web
{
    public class DapperIdentityStarter<TUser, TRole, TIdentityService> : WebStarter
        where TUser : UserBase
        where TRole : RoleBase
        where TIdentityService : IIdentityService
    {
        private Action<IdentityOptions> _configure;

        public DapperIdentityStarter(Action<IdentityOptions> configure)
        {
            _configure = configure;
        }

        public override void ConfigureServices(SchubertServicesBuilder servicesBuilder, SchubertWebOptions options)
        {
            var identitySvcdescriptor = ServiceDescriber.Scoped<IIdentityService, TIdentityService>();
            servicesBuilder.ServiceCollection.AddSmart(identitySvcdescriptor);

            servicesBuilder.ServiceCollection
                        .AddIdentity<TUser, TRole>(iop => _configure?.Invoke(iop))
                        .AddDefaultTokenProviders()
                        .AddDapperStores<TUser, TRole>();
        }

        public override void Start(IApplicationBuilder appBuilder, SchubertWebOptions options)
        {
        }
    }
}
