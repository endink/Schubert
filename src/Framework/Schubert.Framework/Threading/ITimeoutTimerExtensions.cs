using Schubert.Framework.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ITimeoutTimerExtensions
    {
        /// <summary>
        /// 以 <paramref name="delay"/> 设置的时间，启动一个定时任务。
        /// 该任务只执行一次，并在 <paramref name="delay"/> 指定的时间之后执行。
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="task"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static ITimeout NewTimeout(this ITimeoutTimer timer, Action<ITimeout> task, TimeSpan delay)
        {
            Guard.ArgumentNotNull(task, nameof(task));
            return timer.NewTimeout(new DelegateTimerTask(task), delay);
        }

        private class DelegateTimerTask : ITimerTask
        {
            private Action<ITimeout> _func = null;

            public DelegateTimerTask(Action<ITimeout> task)
            {
                _func = task;
            }

            public void Run(ITimeout timeout)
            {
                _func(timeout);
            }
        }
    }
}
