using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Schubert.Framework.Environment.Modules;
using System;
using System.Collections.Generic;

namespace Schubert.Framework.DependencyInjection
{
    /// <summary>
    /// 构建 Schubert 运行环境的 Fluent 编程对象。
    /// </summary>
    public sealed class SchubertServicesBuilder
    {
        private IServiceCollection _serviceCollection;
        private IConfiguration _configuration;

        public HashSet<Guid> AddedModules { get; } = new HashSet<Guid>();

        /// <summary>
        /// 创建 <see cref="SchubertServicesBuilder"/> 类的新实例。
        /// </summary>
        internal SchubertServicesBuilder(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            Guard.ArgumentNotNull(serviceCollection, nameof(serviceCollection));
            Guard.ArgumentNotNull(configuration, nameof(configuration));

            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        /// <summary>
        /// 配置框架参数。
        /// </summary>
        /// <param name="setup"><see cref="Action{T}"/> 对象。</param>
        public void ConfigureOptions(Action<SchubertOptions> setup)
        {
            if (setup != null)
            {
                _serviceCollection.Configure<SchubertOptions>(setup);
            }
        }

        /// <summary>
        /// 配置网络参数。
        /// </summary>
        /// <param name="setup"><see cref="Action{T}"/> 对象。</param>
        public void ConfigureNetwork(Action<NetworkOptions> setup)
        {
            if (setup != null)
            {
                _serviceCollection.Configure(setup);
            }
        }

        public SchubertServicesBuilder UserShellDescriptorManager<TManager>()
            where TManager : IShellDescriptorManager
        {
            this.ServiceCollection.TryAdd(new ServiceDescriptor(typeof(IShellDescriptorManager), typeof(TManager), ServiceLifetime.Transient));
            return this;
        }

       

        public IConfiguration Configuration
        {
            get { return _configuration; }
        }

        public IServiceCollection ServiceCollection
        {
            get { return _serviceCollection; }
        }
    }
}