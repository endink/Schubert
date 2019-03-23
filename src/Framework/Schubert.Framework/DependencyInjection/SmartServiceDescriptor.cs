using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Schubert.Framework.DependencyInjection
{
    /// <summary>
    /// 扩展 <see cref="ServiceDescriber"/> 对象。
    /// 提供更多可操作性，例如进行顶部注册（Insert），是否使用尝试注册（即存在同接口服务时不进行注册）。
    /// </summary>
    public class SmartServiceDescriptor : ServiceDescriptor
    {
        public SmartServiceDescriptor(Type serviceType, object instance)
            : base(serviceType, instance)
        { }

        public SmartServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
            : base(serviceType, (Func<IServiceProvider, object>)factory, lifetime)
        {
        }

        public SmartServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
            : base(serviceType, implementationType, lifetime)
        {

        }

        /// <summary>
        /// 获取或设置一个值，指示该 <see cref="SmartServiceDescriptor"/> 添加到 <see cref="IServiceCollection"/> 时使用 Insert 在列表顶部插入，还是使用 Add 在列表尾部追加。
        /// 同时可以控制是否进行尝试注册（即存在同接口服务时不进行注册），也可以进行服务替换。
        /// </summary>
        public SmartOptions Options { get; set; }

        public static SmartServiceDescriptor Create(ServiceDescriptor descriptor, SmartOptions options = SmartOptions.TryAdd)
        {
            Guard.ArgumentNotNull(descriptor, nameof(descriptor));

            SmartServiceDescriptor rd = null;
            if (descriptor.ImplementationInstance != null)
            {
                rd = new SmartServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationInstance) { Options = options };
            }
            else if (descriptor.ImplementationFactory != null)
            {
                rd = new SmartServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationFactory, descriptor.Lifetime) { Options = options };
            }
            else if (descriptor.ImplementationType != null)
            {
                rd = new SmartServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationType, descriptor.Lifetime) { Options = options };
            }
            return rd;
        }
    }

    /// <summary>
    /// 表示注册服务时使用 Insert 在列表顶部插入，还是使用 Add 在列表尾部追加；
    /// 同时可以控制是否使用使用尝试注册（即存在同接口服务时不进行注册），也可以选择替换现有服务。
    /// </summary>
    public enum SmartOptions
    {
        /// <summary>
        /// 表示在尾部追加。
        /// </summary>
        Append,

        /// <summary>
        /// 表示在尾部尝试添加，如果已经存在其他实现，则不会再进行添加。
        /// </summary>
        TryAdd,

        /// <summary>
        /// 表示在顶部插入。
        /// </summary>
        Insert,

        /// <summary>
        /// 表示先移除列表中具有相同 ServiceType 的第一条服务，然后再追加。
        /// </summary>
        Replace,

        /// <summary>
        /// 表示尝试追加，如果已经存在其他实现会将该实现添加到尾部，如果已存在该实现，则不进行任何操作。
        /// </summary>
        TryAppend
    }
    public static class SmartServiceCollectionExtensions
    {
        /// <summary>
        /// 将 <see cref="ServiceDescriptor"/> 视为可替换的服务，
        /// 在添加服务时自动使用 ServiceCollectionExtensions.TryAdd 方法。
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static ServiceDescriptor AsSmart(this ServiceDescriptor descriptor)
        {
            return SmartServiceDescriptor.Create(descriptor);
        }

        /// <summary> 
        /// 自动根据 descriptor 在注册服务时进行判断是否已经存在指定接口的服务，如果存在会跳过添加，并且可以对注册位置控制。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="descriptor">要注册的 <see cref="ServiceDescriptor"/> 对象。</param> 
        public static void AddSmart(this IServiceCollection collection, SmartServiceDescriptor descriptor)
        {
            if (descriptor == null)
            {
                return;
            }
            switch (descriptor.Options)
            {
                case SmartOptions.Append:
                    collection.Add(descriptor);
                    break;
                case SmartOptions.TryAdd:
                    collection.TryAdd(descriptor);
                    break;
                case SmartOptions.Insert:
                    collection.Insert(0, descriptor);
                    break;
                case SmartOptions.TryAppend:
                    collection.TryAddEnumerable(descriptor);
                    break;
                default:
                    collection.Replace(descriptor);
                    break;
            }
        }

        /// <summary>
        /// 批量注册 <see cref="SmartServiceDescriptor"/> 自动根据 <see cref="SmartServiceDescriptor.Options"/> 属性对注册进行更多控制。 
        /// </summary>、
        /// <param name="collection"></param>
        /// <param name="descriptors">要注册的 <see cref="SmartServiceDescriptor"/> 集合。</param>
        public static void AddSmart(this IServiceCollection collection, IEnumerable<SmartServiceDescriptor> descriptors)
        {
            if (descriptors.IsNullOrEmpty())
            {
                return;
            }
            foreach (var d in descriptors)
            {
                collection.AddSmart(d);
            }
        }
    }
}