using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Events
{
    /// <summary>
    /// 提供描述事件订阅者的特性类（必须作用在公共方法上）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class EventSubscriptionAttribute : Attribute
    {
        /// <summary>
        /// 创建 <see cref="EventSubscriptionAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="eventName">要订阅的事件名称。</param>
        public EventSubscriptionAttribute(string eventName)
        {
            Guard.ArgumentNullOrWhiteSpaceString(eventName, nameof(eventName));

            this.EventName = eventName;
            this.ThreadOptions = ThreadOption.PublisherThread;
            this.Policy = SubscriptionPolicy.Always; 
        }
        
        /// <summary>
        /// 表示生命订阅的的依赖项类型， 通常是从 IDependency 继承的接口类型（当一个类型具有多个可以从 <see cref="IDependency"/> 分配的接口类时使用此属性标识订阅属于那一个依赖项）。
        /// </summary>
        public Type DeclaringDependencyType { get; set; }

        /// <summary>
        /// 事件名称（区分大小写）。
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        /// 事件出发后订阅回调的线程（默认为 <see cref="ThreadOption.PublisherThread"/>）。
        /// </summary>
        public ThreadOption ThreadOptions { get; set; }

        /// <summary>
        /// 事件订阅策略，指示时间订阅回调的次数（默认为 <see cref="SubscriptionPolicy.Always"/>）。
        /// </summary>
        public SubscriptionPolicy Policy { get; set; }
    }
}
