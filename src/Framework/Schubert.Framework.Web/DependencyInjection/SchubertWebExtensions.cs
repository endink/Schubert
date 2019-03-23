using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Schubert.Framework.Caching;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Environment.Modules;
using Schubert.Framework.Json;
using Schubert.Framework.Localization;
using Schubert.Framework.Web;
using Schubert.Framework.Web.Authentication;
using Schubert.Framework.Web.DependencyInjection;
using Schubert.Framework.Web.FileProviders;
using Schubert.Framework.Web.Mvc;
using Schubert.Framework.Web.Mvc.Conventions;
using System;
using System.Linq;

namespace Schubert
{
    public static class SchubertWebExtensions
    {
        public static SchubertWebBuilder _webBuilder = null;
        private static Guid _module = Guid.NewGuid();

        /// <summary>
        /// 启用Schubert 框架的 Web 特性。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setup">加入 Mvc 支持。</param>
        /// <returns></returns>
        public static SchubertServicesBuilder AddWebFeature(this SchubertServicesBuilder services, Action<SchubertWebBuilder> setup = null)
        {
            SchubertWebOptions options = new SchubertWebOptions();
            bool firstInvoke = true;
            if ((firstInvoke = services.AddedModules.Add(_module)))
            {
                IConfiguration configuration = services.Configuration.GetSection("Schubert:Web") as IConfiguration ?? new ConfigurationBuilder().Build();

                services.ServiceCollection.Configure<SchubertWebOptions>(configuration);

                var schubertWebSetup = new SchubertWebOptionsSetup(configuration);
                schubertWebSetup.Configure(options);
            }

            _webBuilder = new SchubertWebBuilder(services);
            setup?.Invoke(_webBuilder);


            if (_webBuilder.FeatureSetup != null)
            {
                services.ServiceCollection.Configure(setup);
            }
            _webBuilder.FeatureSetup?.Invoke(options);
            services.ServiceCollection.AddDataProtection();
            
            services.ServiceCollection.AddLocalization();
            services.ServiceCollection.Replace(ServiceDescriptor.Singleton<IStringLocalizerFactory, SchubertStringLocalizerFactory>());
            services.ServiceCollection.TryAddSingleton<IMemoryCache>(s=> LocalCache.InnerCache);
            services.AddCacheForAspNet();

            var cookieSetup = _webBuilder.CookieSetup;
            services.ServiceCollection.ConfigureApplicationCookie(o =>
            {
                o.LoginPath = "/Login";
                o.LogoutPath = "/LogOff";
                o.Cookie.HttpOnly = true;
                o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                cookieSetup?.Invoke(o);
            });

            var authenticationBuilder = services.ServiceCollection.AddAuthentication();

            if (options.UseCookieAuth)
            {
                authenticationBuilder.AddCookie();
            }
            if (options.UseSession)
            {
                services.ServiceCollection.AddSession(sop =>
                {
                    sop.IdleTimeout = TimeSpan.FromMinutes(options.SessionTimeoutMinutes);
                });
            }
            
            services.ServiceCollection.AddSmart(SchubertWebServices.GetServices(options, firstInvoke));

            foreach (var s in _webBuilder.WebStarters)
            {
                s.ConfigureServices(services, options);
            }

            AddMvc(services, _webBuilder, options);

            return services;
        }
        

        private static IContractResolver GetContractResolver(JsonCaseStyle style, JsonResolverSettings settings)
        {
            switch (style)
            {
                case JsonCaseStyle.CamelCase:
                   
                    return new ExtendedCamelCaseContractResolver(settings);
                case JsonCaseStyle.PascalCase:
                    default:
                    return new ExtendedContractResolver(settings);
            }
        }
        

        private static void AddMvc(SchubertServicesBuilder services, SchubertWebBuilder featureBuilder, SchubertWebOptions options)
        {
            Action<MvcOptions> configure = mvc =>
            {
                if (!options.GlobalRoutePrefix.IsNullOrWhiteSpace())
                {
                    mvc.Conventions.Insert(0, new RoutePrefixConvention(new RouteAttribute(options.GlobalRoutePrefix.Trim())));
                }
            };

            switch (options.MvcFeatures)
            {
                case MvcFeatures.Full:
                    var mvcBuilder = services.ServiceCollection.AddMvc(configure);
                    featureBuilder.MvcSetup?.Invoke(mvcBuilder);
                    mvcBuilder.AddJsonOptions(json=>json.SerializerSettings.ContractResolver = GetContractResolver(options.JsonCaseStyle, options.JsonResolver))
                        .AddRazorOptions(rveo =>
                        {
                            rveo.FileProviders.Insert(0, new ModuleFileProvider(rveo.FileProviders.FirstOrDefault()));
                            rveo.ViewLocationExpanders.Insert(0, new ModuleViewLocationExpander());
                        });
                    //services.ServiceCollection.AddAntiforgery();
                    break;
                case MvcFeatures.Core:
                    var coreBuilder = services.ServiceCollection.AddMvcCore(configure);
                    featureBuilder.MvcCoreSetup?.Invoke(coreBuilder);
                    break;
                case MvcFeatures.Api:
                    var apiBuilder = services.ServiceCollection.AddMvcCore(configure);
                    featureBuilder.MvcCoreSetup?.Invoke(apiBuilder);
                    apiBuilder.AddApiExplorer()
                        .AddAuthorization()
                        .AddFormatterMappings()
                        .AddJsonFormatters(settings => settings.ContractResolver = GetContractResolver(options.JsonCaseStyle, options.JsonResolver))
                        .AddDataAnnotations()
                        .AddCors();
                    featureBuilder.AddWebApiConventions();
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// 启动基于 Schubert 引擎的 Web 应用程序。
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder StartSchubertWebApplication(this IApplicationBuilder builder)
        {
            builder.ApplicationServices.StartSchubertEngine();
            //启动引擎，为我们动态注册服务，创建 Shell 上下文。
            IOptions<SchubertWebOptions> options = builder.ApplicationServices.GetService<IOptions<SchubertWebOptions>>();
            if (options != null && options.Value != null)
            {
                foreach (var s in _webBuilder.WebStarters)
                {
                    s.Start(builder, options.Value);
                }

                var moduleManager = builder.ApplicationServices.GetRequiredService<IModuleManager>();
                
                builder.UseStaticFiles();
                builder.UseAuthentication();

                if (options.Value.UseSession)
                {
                    builder.UseSession();
                }
                if (options.Value.MvcFeatures != MvcFeatures.None)
                {
                    builder.UseMvc();
                }
            }
            //尽可能节省内存，让GC可以回收 WebBuilder
            _webBuilder = null;
            return builder;
        }
    }
}