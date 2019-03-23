using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Environment.Modules;
using Schubert.Framework.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Schubert.Framework.Environment.ShellBuilders
{
    /// <summary>
    /// 创建 <see cref="ShellContext"/> 对象的工厂实例。
    /// </summary>
    public class ShellContextFactory : IShellContextFactory
    {
        private IShellDescriptorCache _shellDescriptorCache = null;
        private IShellDescriptorManager _shellDescriptorManager = null;
        private ICompositionStrategy _compositionStrategy = null;
        private ILogger _logger = null;
        private SchubertOptions _options;

        public ShellContextFactory(
            IShellDescriptorCache shellDescriptorCache, 
            IShellDescriptorManager shellDescriptorManager,
            ICompositionStrategy compositionStrategy,
            IOptions<SchubertOptions> options,
            ILoggerFactory loggerFactory)
        {
            Guard.ArgumentNotNull(shellDescriptorManager, nameof(shellDescriptorManager));
            Guard.ArgumentNotNull(shellDescriptorCache, nameof(shellDescriptorCache));
            Guard.ArgumentNotNull(compositionStrategy, nameof(compositionStrategy));
            Guard.ArgumentNotNull(options, nameof(options));

            _options = options.Value;
            _compositionStrategy = compositionStrategy;
            _shellDescriptorManager = shellDescriptorManager;
            _shellDescriptorCache = shellDescriptorCache;
            _logger = loggerFactory?.CreateLogger<ShellContextFactory>() ?? (ILogger)NullLogger.Instance;
        }

        /// <summary>
        /// 创建 Shell 运行时上下文。
        /// </summary>
        /// <returns>创建的 <see cref="ShellContext"/> 实例。</returns>
        public ShellContext CreateShellContext()
        {
            _logger.WriteTrace($"正在为应用程序 {_options.AppName}（{_options.AppSystemName}） 创建 Shell 。");

            var knownDescriptor = _shellDescriptorCache.Fetch(_options.AppName);
            if (knownDescriptor == null)
            {
                //本地没有存储蓝图信息，则先构建应用程序运行所需的最小蓝图。
                _logger.WriteTrace("找不到缓存的 SehllDescriptor 对象。将使用最小蓝图启动应用程序。");
                knownDescriptor = MinimumShellDescriptor();
            }
            //组装应用程序组件构建蓝图。
            var blueprint = _compositionStrategy.Compose(_options.AppName, knownDescriptor);

            ShellDescriptor currentDescriptor = _shellDescriptorManager.GetShellDescriptor();

            //首次运行 currentDescriptor 可能为空，将用最小蓝图启动程序。
            if (currentDescriptor != null && knownDescriptor.ApplicationName != currentDescriptor.ApplicationName)
            {
                _logger.WriteTrace("获得了新的 Shell 信息。 正在重新创建 Shell 蓝图。");

                _shellDescriptorCache.Store(_options.AppName, currentDescriptor);
                blueprint = _compositionStrategy.Compose(_options.AppName, currentDescriptor);
            }

            //注意，ConfigureBlueprint 为迭代器方法，对属性赋值时切勿直接赋值，应该先保证调用。
            //迭代器方法在实际使用时才会调用，更多请参考 C# 语言知识，这里先 ToArray 防止访问 ShellContext.Services 属性时造成多次调用。
            var descriptor = this.BuildDependencies(blueprint);

            return new ShellContext
            {
                Descriptor = currentDescriptor ?? knownDescriptor,
                Blueprint = blueprint,
                Services = descriptor.Item1,
                EventSubscriptors = descriptor.Item2
            };
        }

        private bool IsDependencyInterface(Type interfaceType)
        {
            return interfaceType.Equals(typeof(IDependency)) || interfaceType.Equals(typeof(ISingletonDependency)) || interfaceType.Equals(typeof(ITransientDependency));
        }

        /// <summary>
        /// 是否只包含三种 <see cref="IDependency"/> 接口
        /// </summary>
        private bool IsDependencyInterfaceOnly(IEnumerable<Type> interfaceTypes)
        {
            int interfaceCount = interfaceTypes.Count();

            return interfaceCount > 0 && (!interfaceTypes.Any(i => !IsDependencyInterface(i))); //所有接口类型中不包含不是 Dependency 接口的类型。
        }

        private IEnumerable<EventSubscriptionDescriptor> CreateDescriptor(Dictionary<Type, MethodInfo[]> methodCache, 
            HashSet<String> flags,
            Type dependencyType, 
            Type instanceType, 
            ServiceLifetime lifetime)
        {
            var methods = methodCache.GetOrAdd(instanceType, t =>
            {
                MethodInfo[] result = 
                    t.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m=>m.GetCustomAttributes<EventSubscriptionAttribute>().Any()).ToArray();
                return result;
            });
            foreach (var m in methods)
            {
                if (m.GetCustomAttributes<EventSubscriptionAttribute>().Any())
                {
                    foreach (var descriptor in EventSubscriptionDescriptor.FromMethod(dependencyType, lifetime, m))
                    {
                        string declaringString = descriptor.IsDeclareDependency ? descriptor.DependencyType.AssemblyQualifiedName : "";
                        if (flags.Add($"{m.DeclaringType.AssemblyQualifiedName}.{m.Name}:{declaringString}"))
                        {
                            _logger.WriteTrace($"发现事件订阅 {m.DeclaringType.FullName}.{m.Name} ( event name: {descriptor.EventName}, dependency:{descriptor.DependencyType} )。");
                            yield return descriptor;
                        }
                        else
                        {
                            throw new SchubertException($"方法 {m.DeclaringType.FullName}.{m.Name} 存在不明确的或重复的事件订阅声明，请考虑使用 {nameof(EventSubscriptionAttribute)}.{nameof(EventSubscriptionAttribute.DeclaringDependencyType)} 属性标识。");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取蓝图中的依赖项和事件。
        /// </summary>
        private Tuple<IEnumerable<SmartServiceDescriptor>, IEnumerable<EventSubscriptionDescriptor>> BuildDependencies(ShellBlueprint blueprint)
        {
            List<SmartServiceDescriptor> smartServiceDescriptors = new List<SmartServiceDescriptor>();
            smartServiceDescriptors.Add(ServiceDescriber.Instance(blueprint, SmartOptions.Replace));
            smartServiceDescriptors.Add(ServiceDescriber.Instance(blueprint.Descriptor, SmartOptions.Replace));

            SetupServiceDescriptors(blueprint, smartServiceDescriptors);

            List<EventSubscriptionDescriptor> eventSubs = new List<EventSubscriptionDescriptor>();
            SetupDependencies(blueprint, smartServiceDescriptors, eventSubs);
            return new Tuple<IEnumerable<SmartServiceDescriptor>, IEnumerable<EventSubscriptionDescriptor>>(smartServiceDescriptors, eventSubs);
        }

        private void SetupDependencies(ShellBlueprint blueprint, List<SmartServiceDescriptor> smartServiceDescriptors, List<EventSubscriptionDescriptor> eventSubs)
        {
            Dictionary<Type, MethodInfo[]> methodsCache = new Dictionary<Type, MethodInfo[]>();
            HashSet<String> flags = new HashSet<String>(); //处理重复项。
            foreach (var item in blueprint.Dependencies.Cast<ShellBlueprintDependencyItem>())
            {
                var interfaceTypes = item.Type.GetTypeInfo().GetInterfaces().Where(itf => typeof(IDependency).GetTypeInfo().IsAssignableFrom(itf)).ToArray();
                if (interfaceTypes.Length == 0)
                {
                    continue;
                }
                bool useDependencyInterfaceOnly = IsDependencyInterfaceOnly(interfaceTypes);
                

                if (useDependencyInterfaceOnly)
                {
                    this.RegisterBluePrintItem(smartServiceDescriptors, eventSubs, methodsCache, flags, item, item.Type);
                }
                else
                {
                    //注册 IDependency，特别注意，如果一类型上直接使用了 IDependency 接口，理解为想要直接注入该类型。
                    foreach (var interfaceType in interfaceTypes.Where(i => !IsDependencyInterface(i)))
                    {
                        var interfaceInfo = interfaceType.GetTypeInfo();
                        var itemInfo = item.Type.GetTypeInfo();
                        if (!interfaceInfo.IsGenericType || (interfaceInfo.IsGenericTypeDefinition && itemInfo.IsGenericTypeDefinition)) //不能是非定义的泛型接口
                        {
                            RegisterBluePrintItem(smartServiceDescriptors, eventSubs, methodsCache, flags, item, interfaceType);
                        }
                    }
                }
            }
        }

        private void RegisterBluePrintItem(List<SmartServiceDescriptor> smartServiceDescriptors, List<EventSubscriptionDescriptor> eventSubs, Dictionary<Type, MethodInfo[]> methodsCache, HashSet<string> flags, ShellBlueprintDependencyItem item, Type interfaceType)
        {
            SmartServiceDescriptor descriptor = null;
            if (typeof(ISingletonDependency).GetTypeInfo().IsAssignableFrom(interfaceType))
            {
                eventSubs.AddRange(CreateDescriptor(methodsCache, flags, interfaceType, item.Type, ServiceLifetime.Singleton));
                descriptor = ServiceDescriber.Singleton(interfaceType, item.Type, SmartOptions.Append);
                item.Interfaces.Add(new Tuple<Type, ServiceLifetime>(interfaceType, ServiceLifetime.Singleton));
            }
            else if (typeof(ITransientDependency).GetTypeInfo().IsAssignableFrom(interfaceType))
            {
                eventSubs.AddRange(CreateDescriptor(methodsCache, flags, interfaceType, item.Type, ServiceLifetime.Transient));
                descriptor = ServiceDescriber.Transient(interfaceType, item.Type, SmartOptions.Append);
                item.Interfaces.Add(new Tuple<Type, ServiceLifetime>(interfaceType, ServiceLifetime.Transient));
            }
            else
            {
                eventSubs.AddRange(CreateDescriptor(methodsCache, flags, interfaceType, item.Type, ServiceLifetime.Scoped));
                descriptor = ServiceDescriber.Scoped(interfaceType, item.Type, SmartOptions.Append);
                item.Interfaces.Add(new Tuple<Type, ServiceLifetime>(interfaceType, ServiceLifetime.Scoped));
            }

            if (descriptor != null)
            {
                descriptor = this.OnDependencyRegistering(descriptor);
                smartServiceDescriptors.Add(descriptor);
            }
        }

        private SmartServiceDescriptor OnDependencyRegistering(SmartServiceDescriptor serviceDescriptor)
        {
            DependencySetupEventArgs args = new DependencySetupEventArgs(serviceDescriptor);
            ShellEvents.NotifyDependencyRegistering(_options, args);
            var newDesc = args.ActualDependency ?? args.OriginalDependency;
            return Object.ReferenceEquals(newDesc, serviceDescriptor) ? serviceDescriptor : SmartServiceDescriptor.Create(newDesc, serviceDescriptor.Options);
        }

        private static void SetupServiceDescriptors(ShellBlueprint blueprint, List<SmartServiceDescriptor> smartServiceDescriptors)
        {
            foreach (var item in blueprint.DependencyDescribers)
            {
                var constructor = item.Type.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(c => c.GetParameters().Length == 0);
                if (constructor == null)
                {
                    throw new SchubertException($@"类型 {item.Type.FullName} 实现了 {nameof(IDependencyDescriber)} 接口，必须提供公共无参构造函数。");
                }
                IDependencyDescriber d = (IDependencyDescriber)Activator.CreateInstance(item.Type);
                foreach (var descriptor in d.Build())
                {
                    smartServiceDescriptors.Add(descriptor);
                }
            }
        }


        /// <summary>
        /// 运行时环境最小化实例。
        /// </summary>
        /// <returns></returns>
        private ShellDescriptor MinimumShellDescriptor()
        {
            return new ShellDescriptor
            {
                ApplicationName = _options.AppSystemName.IfNullOrWhiteSpace("SchubertApp"),
                DisabledFeatures = new[] { CompositionStrategy.FrameworkFeatureName }
            };
        }

        public ShellContext CreateSetupContext()
        {
            throw new NotSupportedException();
        }
    }
}