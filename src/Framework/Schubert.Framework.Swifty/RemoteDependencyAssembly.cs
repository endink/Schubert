using Microsoft.Extensions.Options;
using Swifty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.DependencyInjection;
using Swifty.MicroServices.Ribbon;
using Swifty.Nifty.Client;

namespace Schubert.Framework.Swifty
{
    internal class RemoteDependencyAssembly
    {
        private static readonly Type SwiftyRemoteServiceType = typeof(RemoteServiceAttribute);
        private static readonly Type ThriftServiceType = typeof(ThriftServiceAttribute);
        
        private readonly SwiftyOptions _options;

        public RemoteDependencyAssembly(SwiftyOptions options, Assembly assembly)
        {
            _options = options;

            this.RawAssembly = assembly;
        }
        

        private bool IsThriftService(Type serviceType)
        {
            return serviceType.HasAttribute<ThriftServiceAttribute>();
        }


        private Assembly RawAssembly { get; }
        
        /// <summary>
        /// 导出远程调用的依赖项。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SmartServiceDescriptor> ExportDependencies(SwiftyOptions swiftyOptions)
        {
            var thriftServices = this.RawAssembly.ExportedTypes
                 .Where(t => t.GetTypeInfo().IsInterface && IsThriftService(t));

            Object GetRemoteServiceInstance(IServiceProvider serviceProvider, RemoteServiceAttribute removeAttribute, Type thriftInterface)
            {
                var clientManager = serviceProvider.GetRequiredService<SwiftyClientManager>();
                ClientSslConfig ssl = swiftyOptions.Client.GetSslConfig(removeAttribute.VipAddress);

                if (this._options.Client.TryGetDirectAddress(removeAttribute.VipAddress, out string address))
                {
                    
                    return clientManager.Client.Create(thriftInterface, address, ssl);
                }
                return clientManager.Client.Create(thriftInterface, removeAttribute.Version, removeAttribute.VipAddress, ssl);
            }


            foreach (var thriftService in thriftServices)
            {
                if (thriftService.TryGetRemoteServiceAttribute(true, out RemoteServiceAttribute attribute))
                {
                    yield return new SmartServiceDescriptor(thriftService, sp => GetRemoteServiceInstance(sp, attribute, thriftService), ServiceLifetime.Singleton)
                    { Options = SmartOptions.TryAppend };
                }
            }
        }
    }
}
