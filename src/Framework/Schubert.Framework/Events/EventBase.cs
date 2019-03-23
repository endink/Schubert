using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Schubert.Framework.Environment;

namespace Schubert.Framework.Events
{
    ///<summary>
    /// 定义一个事件的基类。
    ///</summary>
    public abstract class EventBase
    {
        private readonly IList<EventSubscriptionDescriptor> _subscriptions;
        private IServiceProvider _serviceProvider;

        public EventBase(IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            _subscriptions = new List<EventSubscriptionDescriptor>();
            _serviceProvider = serviceProvider;
        }
        
        /// <summary>
        /// 获取当前的订阅列表。
        /// </summary>
        /// <value>The current subscribers.</value>
        protected ICollection<EventSubscriptionDescriptor> Subscriptions
        {
            get { return _subscriptions; }
        }
        
        protected internal virtual void SubscribeCore(EventSubscriptionDescriptor eventSubscription)
        {
            Guard.ArgumentNotNull(eventSubscription, nameof(eventSubscription));

            lock (Subscriptions)
            {
                Subscriptions.Add(eventSubscription);
            }
        }

        /// <summary>
        /// 回调订阅的 <see cref="EventSubscriptionDescriptor"/> 列表。
        /// </summary>
        protected virtual void NotifyCore(object sender, object args)
        {
            var array = _subscriptions.ToArray();
            Parallel.ForEach(array, s =>
            {
                if (s.ThreadOptions == ThreadOption.PublisherThread)
                {
                    ExecuteAndReleaseHandler(sender, args, s);
                }
                else
                {
                    Task.Run(() =>
                    {
                        ExecuteAndReleaseHandler(sender, args, s);
                    });
                }
            });
        }

        private void ExecuteAndReleaseHandler(object sender, object args, EventSubscriptionDescriptor s)
        {
            IServiceScope scope = null;
            try
            {
                object instance = null;
                if (s.Lifetime == ServiceLifetime.Scoped) //对于生命周期为一个下文的我们先尝试查找应用程序中的上下文尝试来加载。
                {
                    IWorkContextAccessor workContext = _serviceProvider.GetRequiredService<IWorkContextAccessor>();
                    var context = workContext.GetContext();
                    if (context == null)
                    {
                        IServiceScopeFactory fac = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                        scope = fac.CreateScope();
                        Type dependenyType = s.DependencyType;
                        instance = scope.ServiceProvider.GetRequiredService(s.DependencyType);
                    }
                    else
                    {
                        instance = context.ResolveRequired(s.DependencyType);
                    }
                }
                else
                {
                    instance = _serviceProvider.GetRequiredService(s.DependencyType);
                }
                try
                {
                    s.Caller.DynamicInvoke(instance, sender, args);
                }
                catch(ArgumentException)
                {
                    throw new SchubertException($@"事件订阅方法中的事件参数和事件发布的参数不匹配。{System.Environment.NewLine} 事件 ""{s.EventName}"" 要求订阅方法 {s.DependencyType}.{s.MethodName} 签名必须为 void (object sender, {args?.GetType() ?? typeof(object)})");
                }
                if (s.Policy == SubscriptionPolicy.Once)
                {
                    this.UnsubscribeCore(s);
                }
            }
            finally
            {
                if (scope != null)
                {
                    scope.Dispose();
                }
                if (s.Lifetime == ServiceLifetime.Transient)
                {
                    IDisposable disposable = sender as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        protected internal virtual void UnsubscribeCore(EventSubscriptionDescriptor descriptor)
        {
            lock (Subscriptions)
            {
                EventSubscriptionDescriptor subscription = Subscriptions.FirstOrDefault(evt => evt.Identity.CaseSensitiveEquals(descriptor.Identity));
                if (subscription != null)
                {
                    Subscriptions.Remove(subscription);
                }
            }
        }

        /// <summary>
        /// 判断订阅列表中是否存在订阅引用。
        /// </summary>
        /// <param name="descriptor">一个订阅引用。</param>
        /// <returns>如果过订阅列表中存在 <paramref name="descriptor"/>，则返回 <see langword="true"/>。</returns>
        protected virtual bool ContainsCore(EventSubscriptionDescriptor descriptor)
        {
            lock (Subscriptions)
            {
                return Subscriptions.Any(evt => evt.Identity.CaseSensitiveEquals(descriptor.Identity));
            }
        }
    }
}