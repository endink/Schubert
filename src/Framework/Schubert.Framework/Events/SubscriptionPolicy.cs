using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Events
{
    /// <summary>
    /// 表示订阅执行的策略。
    /// </summary>
    public enum SubscriptionPolicy
    {
        /// <summary>
        /// 每次触发事件都会执行。
        /// </summary>
        Always,
        /// <summary>
        /// 仅执行一次（在第一次触发事件时会执行，后续触发将会略）。
        /// </summary>
        Once
    }
}
