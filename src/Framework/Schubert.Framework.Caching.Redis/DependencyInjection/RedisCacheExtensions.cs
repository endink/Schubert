using Microsoft.Extensions.Configuration;
using Schubert.Framework.Caching;
using System;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.DependencyInjection;
using Microsoft.Extensions.Options;
using Schubert.Framework;

namespace Schubert
{
    public static class RedisCacheExtensions
    {
        private static Guid _module = Guid.NewGuid();

        /// <summary>
        /// 使用 Redis 缓存服务（将以 Redis 缓存实现 <see cref="ICacheManager"/> 接口，默认连接配置节为 Schubert:Redis。）。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="setup"> Redis 缓存的配置安装方法。</param>
        /// <returns></returns>
        public static SchubertServicesBuilder AddRedisCache(this SchubertServicesBuilder builder, Action<SchubertRedisOptions> setup = null)
        {
            var configuration = builder.Configuration.GetSection("Schubert:Redis") as IConfiguration ?? new ConfigurationBuilder().Build();
            
            builder.ServiceCollection.Configure<SchubertRedisOptions>(configuration);

            SchubertRedisOptions options = new SchubertRedisOptions();
            var redisSetup = new ConfigureFromConfigurationOptions<SchubertRedisOptions>(configuration);
            redisSetup.Configure(options);
            if (setup != null)
            {
                setup(options);
                builder.ServiceCollection.Configure(setup);
            }
            
            builder.ServiceCollection.AddSmart(ServiceDescriber.Singleton<ICacheManager, RedisCacheManager>(SmartOptions.Replace));
            if (builder.AddedModules.Add(_module))
            {
                builder.ServiceCollection.AddSmart(ServiceDescriber.Singleton<IRedisCacheSerializer, JsonNetSerializer>(SmartOptions.Append));
            }

            if (options.ConnectionString.IsNullOrWhiteSpace())
            {
                throw new SchubertException("必须为 RedisCacheManager 指定连接字符串，可以通过 Schubert:Redis:ConnectionString 配置节配置。");
            }
            
            return builder;
        }

        /// <summary>
        /// 配置 Schubert 框架的 Redis 缓存服务。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="setup">表示配置操作的方法。</param>
        public static void ConfigureRedisCache(this IServiceCollection builder, Action<SchubertRedisOptions> setup = null)
        {
            if (setup != null)
            {
                builder.Configure(setup);
            }
        }
    }
}
