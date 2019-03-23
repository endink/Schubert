using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Threading
{
   
    /// <summary>
    ///  实现一次调度程序
    /// </summary>
    public interface ITimeoutTimer : IDisposable
    {

        /// <summary>
        /// 在 <paramref name="delay"/> 置顶的延时之后执行一次性调度任务。
        /// </summary>
        /// <param name="task">要执行的任务。</param>
        /// <param name="delay">延迟时间。</param>
        /// <returns>一个  <see cref="ITimeout"/> 对象。</returns>
        ITimeout NewTimeout(ITimerTask task, TimeSpan delay);
        
        /// <summary>
        /// 取消调度器中已经添加排队但没有执行的任务。
        /// </summary>
        /// <returns></returns>
        IEnumerable<ITimeout> Stop();
    }
}
