using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Threading
{
    public interface ITimeout
    {

        /// <summary>
        /// 获取当前 <see cref="ITimeout"/> 所属的调度器。
        /// </summary>
        ITimeoutTimer Timer { get; }

        /// <summary>
        /// 和当前 <see cref="ITimeout"/> 关联的 <see cref="ITimerTask"/>
        /// </summary>
        ITimerTask Task { get; }

        /// <summary>
        /// 获取一个值，指示当前任务是否超时。
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// 获个值，指示当前任务是否已被取消。
        /// </summary>
        bool IsCancelled { get; }

        /// <summary>
        /// 取消当前关联的任务，如果任务已经被取消或已经执行，该调用不会产生任何作用。
        /// </summary>
        void Cancel();
    }
}
