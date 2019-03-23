using Microsoft.Extensions.DependencyInjection;
using Schubert.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Schubert.Framework.Events
{
    ///<summary>
    /// 描述 <see cref="EventBase"/> 的一个订阅。
    ///</summary>
    public class EventSubscriptionDescriptor
    {
        internal Delegate Caller { get; private set; }

        public ThreadOption ThreadOptions { get; private set; }

        public SubscriptionPolicy Policy { get; set; }

        public string EventName { get; private set; }

        public ServiceLifetime Lifetime { get; internal set; }

        public string Identity { get; internal set; }

        public Type DependencyType { get; internal set; }

        public bool IsDeclareDependency { get; internal set; }

        public string MethodName { get; internal set; }

        private static Delegate BuildDelegate(MethodInfo method)
        {
            Delegate @delegate;
            var parameters = method.GetParameters().ToArray();
            if (parameters.Length == 2 && parameters[0].ParameterType.Equals(typeof(Object)))
            {
                var parameter = Expression.Parameter(method.DeclaringType, "o");
                var sender = Expression.Parameter(typeof(object), "sender");
                var args = Expression.Parameter(parameters[1].ParameterType, "args");
                var methodCall = Expression.Call(parameter, method, sender, args);
                @delegate = Expression.Lambda(methodCall, parameter, sender, args).Compile();
            }
            else
            {
                throw new SchubertException($"{nameof(EventSubscriptionAttribute)} 标记的事件订阅方法 {method.DeclaringType.FullName}.{method.Name} 不满足方法签名 void (object sender, XXX args)，其中 XXX 为事件参数类型。");
            }

            return @delegate;
        }

        /// <summary>
        /// 从一个订阅类型创建事件订阅的描述信息。
        /// </summary>
        /// <param name="dependencyType">依赖项声明类型。</param>
        /// <param name="lifetime">依赖项生命周期。</param>
        /// <param name="subscriptionMethod">订阅方法。</param>
        /// <returns></returns>
        public static IEnumerable<EventSubscriptionDescriptor> FromMethod(Type dependencyType, ServiceLifetime lifetime, MethodInfo subscriptionMethod)
        {
            Guard.ArgumentNotNull(subscriptionMethod, nameof(subscriptionMethod));

            var attributes = subscriptionMethod.GetCustomAttributes<EventSubscriptionAttribute>(false);
            if (attributes.IsNullOrEmpty())
            {
                throw new ArgumentException($"事件回调类型 {subscriptionMethod.Name} 缺少订阅描述信息，可能由于没有应用 {nameof(EventSubscriptionAttribute)} 属性。", nameof(subscriptionMethod));
            }
            foreach (var attribute in attributes)
            {
                if (attribute.DeclaringDependencyType == null || attribute.DeclaringDependencyType.Equals(dependencyType))
                {
                    var descriptor = new EventSubscriptionDescriptor();

                    descriptor.MethodName = subscriptionMethod.Name;
                    descriptor.Caller = BuildDelegate(subscriptionMethod);
                    descriptor.ThreadOptions = attribute.ThreadOptions;
                    descriptor.Policy = attribute.Policy;
                    descriptor.EventName = attribute.EventName;
                    descriptor.Lifetime = lifetime;
                    descriptor.DependencyType = attribute.DeclaringDependencyType ?? dependencyType;
                    descriptor.IsDeclareDependency = attribute.DeclaringDependencyType != null;
                    
                    descriptor.Identity = CryptoHelper.Encrypt32MD5($"{descriptor.EventName}:{descriptor.DependencyType.AssemblyQualifiedName}{subscriptionMethod.DeclaringType.AssemblyQualifiedName}.{subscriptionMethod.Name}");

                    yield return descriptor;
                }
            }
        }
    }

    
}