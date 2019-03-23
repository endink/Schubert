using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Events
{
    /// <summary>
    /// 实现模块内事件通知的接口。
    /// </summary>
    public interface IEventNotification
    {
        /// <summary>
        /// 进行事件通知。
        /// </summary>
        /// <param name="eventName">要通知的事件名称。</param>
        /// <param name="sender">事件发送者。</param>
        /// <param name="args">事件参数。</param>
        void Notify(string eventName, object sender, object args);
    }
}
