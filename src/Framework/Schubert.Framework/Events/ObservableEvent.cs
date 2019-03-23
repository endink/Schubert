using Schubert.Framework.Environment;
using System;
using System.Linq;

namespace Schubert.Framework.Events
{
    /// <summary>
    /// 表示一个发布订阅模型中的事件，可以用于管理模型中的发布和订阅。
    /// </summary>
    public class ObservableEvent : EventBase
    {
        public ObservableEvent(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            
        }
        /// <summary>
        /// 触发一个 <see cref="ObservableEvent"/> 事件。
        /// </summary>
        /// <param name="sender">触发事件的对象。</param>
        /// <param name="args">事件参数。</param>
        public void Notify(object sender, object args)
        {
            base.NotifyCore(sender, args);
        }
    }
}