using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Swifty;
using Schubert.Framework.Swifty.ContextModel;
using Schubert.Framework.Swifty.DependencyInjection;
using Swifty;
using Swifty.MicroServices.Commons;
using Swifty.MicroServices.Server;
using Swifty.Services;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Schubert.Framework;

namespace Schubert
{

    public static class SwiftyExtensions
    {
        private static ConcurrentBag<SwiftyServiceDescriptor> _serviceExports;
        private static List<SmartServiceDescriptor> _clientExports;

        /// <summary>
        /// 向服务容器中添加 Swifty （基于 Thrift 的 RPC）服务。
        /// 可以使用 <see cref="SwiftyOptions.EnableFeatures"/> 控制是否启用客户端/服务端功能。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="setupAction">对 Swifty 进行配置的委托。</param>
        public static void AddSwifty(this SchubertServicesBuilder builder, Action<SwiftyOptions> setupAction = null)
        {
            SwiftyOptions touchOptions = AddSwiftyOptions(builder, setupAction);

            //启动服务端。
            if (touchOptions.EnableFeatures.HasFlag(SwiftyFeatures.Server))
            {
                AddSwiftyServer();
            }

            if (touchOptions.EnableFeatures.HasFlag(SwiftyFeatures.Client) /*&& !touchOptions.Client.ExploringAssemblies.IsNullOrEmpty()*/)
            {
                AddSwiftyClient(touchOptions);
            }

        }

        private static SwiftyOptions AddSwiftyOptions(SchubertServicesBuilder builder, Action<SwiftyOptions> setupAction)
        {
            SwiftyOptions touchOptions = new SwiftyOptions();

            var configuration = builder.Configuration.GetSection("Schubert:Swifty") as IConfiguration ?? new ConfigurationBuilder().Build();

            var schubertDataSetup = new ConfigureFromConfigurationOptions<SwiftyOptions>(configuration);
            builder.ServiceCollection.Configure<SwiftyOptions>(configuration);

            schubertDataSetup.Configure(touchOptions);

            if (setupAction != null)
            {
                builder.ServiceCollection.Configure(setupAction);
                setupAction(touchOptions);
            }
            builder.ServiceCollection.AddSmart(SwiftyServices.GetServices());
            return touchOptions;
        }

        private static void AddSwiftyServer()
        {
            ShellEvents.DependencyRegistering -= ShellEvents_DependencyRegistering;
            ShellEvents.EngineStarted -= ShellEvents_EngineStarted;

            _serviceExports = new ConcurrentBag<SwiftyServiceDescriptor>();

            ShellEvents.DependencyRegistering += ShellEvents_DependencyRegistering;
            ShellEvents.EngineStarted += ShellEvents_EngineStarted;
        }

        private static void AddSwiftyClient(SwiftyOptions touchOptions)
        {
            ShellEvents.ShellInitialized += ShellEvents_ShellInitialized;
            _clientExports = new List<SmartServiceDescriptor>();

            var assemblies = touchOptions.Client.ExploringAssemblies.Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(name => Assembly.Load(new AssemblyName(name))).Where(a => a != null).ToArray();

            Parallel.ForEach(assemblies, a =>
            {
                RemoteDependencyAssembly remoteAssembly = new RemoteDependencyAssembly(touchOptions, a);
                _clientExports.AddRange(remoteAssembly.ExportDependencies(touchOptions));
            });
        }

        //使用这个事件注册，确保之前的服务端已经注册，这样我们通过 TryAdd 语义排除掉已经注册的服务端接口。
        private static void ShellEvents_ShellInitialized(SchubertOptions options, Framework.Environment.ShellBuilders.ShellContext context)
        {
            if (_clientExports != null /*&& _clientExports.Count > 0*/)
            {
                context.RegisteredServices.AddSmart(_clientExports);
                context.RegisteredServices.AddSingleton<SwiftyClientManager, SwiftyClientManager>();
            }
        }

        private static void ShellEvents_EngineStarted(SchubertOptions options, IServiceProvider serviceProvider)
        {
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger("Schubert");
            //这里启动 Swifty 服务端。
            if (!(_serviceExports?.IsEmpty ?? true)) //其实不可能为空，防止手贱的反射该方法报错。
            {
                IOptions<SwiftyOptions> swiftyOptions = serviceProvider.GetRequiredService<IOptions<SwiftyOptions>>();
                IOptions<NetworkOptions> netOptions = serviceProvider.GetRequiredService<IOptions<NetworkOptions>>();

                /***************
                 * 
                 * 地址是个棘手的问题
                 * 
                 * 1、bindingAddress 为指定的 ip 地址：
                 * 此时我们默认使用这个绑定地址即是他想要注册的地址。
                 * 
                 * 2、考虑 bindingAddress 为空的情况：
                 * 为空时候认为他要使用 network 配置，如果要使用 binding any 必须显式设置为 0.0.0.0
                 * 因此，当 bindingAddress 为空时候我们就默认也注册 network 配置的地址
                
                 * 3、再来一种情况，考虑 bindingAddress 设置为  any（即 0.0.0.0） :
                 * 这时候总不能向注册中心去注册 0.0.0.0 吧
                 * 因此提供一个 public address 单独作为注册地址才能完整的解决问题
                 * 考虑当 public address 为空，bind address 指定了 0.0.0.0：
                 * 这种情况直接再读一次 network 配置，认为他要 bind 端口是本机所有网卡，但是只注册 network 的配置。
                 * 
                 * 似乎没有什么逻辑漏洞，有的话 Swifty 里会再次为我们检查，详情见 swifty 实现。
                 * 
                ***************/

                if (swiftyOptions.Value.Server.BindingAddress.IsNullOrWhiteSpace())
                {
                    swiftyOptions.Value.Server.BindingAddress = GetNetworkAddress(netOptions);
                }
                var address = swiftyOptions.Value.Server.PublicAddress.IfNullOrWhiteSpace(swiftyOptions.Value.Server.BindingAddress);
                address = address.CaseSensitiveEquals("0.0.0.0") ? GetNetworkAddress(netOptions) : address;


                bool enableEureka = !String.IsNullOrWhiteSpace(swiftyOptions.Value.Server.Eureka.EurekaServerServiceUrls);

                if (address.IsNullOrWhiteSpace() && enableEureka)
                {
                    string optionProperty = $"{nameof(SwiftyOptions)}.{nameof(SwiftyOptions.Server)}.{nameof(ExtendedSwiftyServerOptions.PublicAddress)}";
                    string bindProperty = $"{nameof(SwiftyOptions)}.{nameof(SwiftyOptions.Server)}.{nameof(ExtendedSwiftyServerOptions.BindingAddress)}";

                    string error = $"无法确定 Swifty 服务要发布的地址，考虑以下三种方式：{System.Environment.NewLine}" +
                        $"1、请使用 '{nameof(NetworkOptions)}' 配置本机网络。{System.Environment.NewLine}" +
                        $"2、使用 '{optionProperty}' 属性配置注册地址。{System.Environment.NewLine}" +
                        $"3、通过'{bindProperty}' 属性指定明确的 TCP 绑定地址。{System.Environment.NewLine}";

                    throw new SchubertException(error);
                }
                InstanceDescription desc = new InstanceDescription($"{options.AppSystemName}", $"{options.Group}.{options.AppSystemName}", address);
                IServiceLocator locator = new SchubertServiceLocator(loggerFactory);

                SwiftyBootstrap boot = new SwiftyBootstrap(locator, swiftyOptions.Value.Server, desc, loggerFactory);

                var servies = _serviceExports.ToArray();
                //其实也清理不了什么，上一步的对象已经被引用了，清理一个 ConcurrentBag 结构吧。
                _serviceExports = null;

                boot
                    .Handles(handlers => handlers.Add(new ScopeInjectionEventHandler()))
                    .EurekaConfig(swiftyOptions.Value.Server.EnableEureka, swiftyOptions.Value.Server.Eureka)
                    .AddServices(servies)
                    .Bind(address, swiftyOptions.Value.Server.Port)
                    .StartAsync().ContinueWith(s=> 
                    {
                        if (s.Exception != null)
                        {
                            logger.WriteError(0, "启动 swifty 服务器发生错误。", s.Exception);
                        }
                        else
                        {
                            logger.WriteInformation(0, $"swifty 服务器已经启动。{System.Environment.NewLine}" +
                                $"（port: {s.Result.Port},  services: {servies.Length},  eureka: {enableEureka.ToString().ToLower()}）。");
                        }
                    });
            }
            else
            {
                logger.WriteWarning($"开启了 Swifty 服务端功能，但是没有发现可用的 Swifty 服务，是否在接口上遗漏了 {nameof(RemoteServiceAttribute)} 或 {nameof(ThriftServiceAttribute)}。");
            }
        }


        private static String GetNetworkAddress(IOptions<NetworkOptions> netOptions)
        {
            netOptions.Value.TryGetIPAddress(out string networkAddress);
            return networkAddress;
        }

        private static void ShellEvents_DependencyRegistering(SchubertOptions option, DependencySetupEventArgs eventArgs)
        {
            //这里捞出要注册的服务。
            if (TryGetRemoteService(eventArgs.ActualDependency, option, out RemoteServiceAttribute remoteAttribute))
            {
                _serviceExports.Add(new SwiftyServiceDescriptor(eventArgs.ActualDependency.ServiceType, remoteAttribute.Version.IfNullOrWhiteSpace("1.0.0")));
            }
        }

        private static bool TryGetRemoteService(ServiceDescriptor serverTypeDesc, SchubertOptions options, out RemoteServiceAttribute attribute)
        {
            if (serverTypeDesc.ServiceType.TryGetRemoteServiceAttribute(true, out attribute))
            {
                if (!attribute.VipAddress.CaseInsensitiveEquals($"{options.Group}.{options.AppSystemName}"))
                {
                    var svcType = serverTypeDesc.ServiceType.GetTypeInfo();

                    String optionsName = nameof(SchubertOptions);
                    throw new SwiftyApplicationException($"{nameof(RemoteServiceAttribute)} 的属性" +
                        $" {nameof(RemoteServiceAttribute.VipAddress)} 必须使用 [{optionsName}.{nameof(SchubertOptions.Group)}].[{optionsName}.{nameof(SchubertOptions.AppSystemName)}] （区分大小写）格式。{System.Environment.NewLine}" +
                        $"类型 '{svcType.FullName}' 或程序集 '{svcType.Assembly.FullName}' 上的 {nameof(RemoteServiceAttribute)} 不满足该要求。");
                }

                return true;
            }
            return false;
        }
    }
}
