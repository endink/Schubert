using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Schubert.Framework.Configuration;
using Schubert.Framework.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// 应用配置中心中存储的配置。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configBasePath">配置中心配置文件的基目录</param>
        /// <param name="serverConfigFile">配置中心服务器配置文件。</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddConfigurationCenter(this IConfigurationBuilder builder, string configBasePath, string serverConfigFile = "appsettings.json")
        {
            //如果使用ConfigurationBuilder则无法使用链式调用
            Guard.ArgumentNullOrWhiteSpaceString(serverConfigFile, nameof(serverConfigFile));

            var tempBuilder = new ConfigurationBuilder();
            tempBuilder
                .SetBasePath(configBasePath)
                .AddEnvironmentVariables()
                .AddJsonFile(serverConfigFile, false);

            IConfiguration configuration = tempBuilder.Build().GetSection("Schubert:Configuration") as IConfiguration ?? new ConfigurationBuilder().Build();
            if (configuration != null)
            {
                ConfigurationCenterOptions ccOptions = new ConfigurationCenterOptions();
                var serverOptionsSetup = new ConfigureFromConfigurationOptions<ConfigurationCenterOptions>(configuration);
                serverOptionsSetup.Configure(ccOptions);

                builder.Add(new ZookeeperConfigurationSource(ccOptions));
            }
            else
            {
                throw new ConfigurationException($"配置中心服务器配置文件格式不正确，缺少必要的配置节 Schubert:Configuration。");
            }
            return builder;
        }

        /// <summary>
        /// 应用配置中心中存储的配置。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="setup">配置中心配置。</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddConfigurationCenter(this IConfigurationBuilder builder, Action<ConfigurationCenterOptions> setup)
        {
            Guard.ArgumentNotNull(setup, nameof(setup));

            ConfigurationCenterOptions ccOptions = new ConfigurationCenterOptions();
            setup(ccOptions);
            builder.Add(new ZookeeperConfigurationSource(ccOptions));
            return builder;
        }
    }
}
