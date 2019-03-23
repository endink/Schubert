using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Threading
{
    partial class HashedWheelTimer
    {
        internal sealed class HashedWheelTimeout : ITimeout, IDisposable
        {
            public const int ST_INIT = 0;
            public const int ST_IN_BUCKET = 1;
            public const int ST_CANCELLED = 2;
            public const int ST_EXPIRED = 3;

            private HashedWheelTimer _timer;
            private ITimerTask _task;

            private int state = ST_INIT;

            
            public HashedWheelTimeout(HashedWheelTimer timer, ITimerTask task, long deadline)
            {
                this._timer = timer;
                this._task = task;
                this.Deadline = deadline;
            }

            internal long Deadline { get; }

            // remainingRounds will be calculated and set by Worker.transferTimeoutsToBuckets() before the
            // HashedWheelTimeout will be added to the correct HashedWheelBucket.
            internal long RemainingRounds { get; set; }

            // The bucket to which the timeout was added
            internal HashedWheelBucket Bucket { get; set; }

            // This will be used to chain timeouts in HashedWheelTimerBucket via a double-linked-list.
            // As only the workerThread will act on it there is no need for synchronization / volatile.
            internal HashedWheelTimeout Next { get; set; }
            internal HashedWheelTimeout Prev { get; set; }

            public ITimeoutTimer Timer
            {
                get { return this._timer; }
            }

            public ITimerTask Task
            {
                get { return this._task; }
            }

            public void Cancel()
            {
                int state = this.State;
                if (state >= ST_CANCELLED)
                {
                    // fail fast if the task was cancelled or expired before.
                    return;
                }
                if (state != ST_IN_BUCKET && this.CompareAndSetState(ST_INIT, ST_CANCELLED))
                {
                    // Was cancelled before the HashedWheelTimeout was added to its HashedWheelBucket.
                    // In this case we can just return here as it will be discarded by the WorkerThread when handling
                    // the adding of HashedWheelTimeout to the HashedWheelBuckets.
                    return;
                }
                // only update the state it will be removed from HashedWheelBucket on next tick.
                if (!CompareAndSetState(ST_IN_BUCKET, ST_CANCELLED))
                {
                    return;
                }
                // Add the HashedWheelTimeout back to the timeouts queue so it will be picked up on the next tick
                // and remove this HashedTimeTask from the HashedWheelBucket. After this is done it is ready to get
                // GC'ed once the user has no reference to it anymore.
                _timer._timeouts.Enqueue(this);
            }

            public void Remove()
            {
                if (Bucket != null)
                {
                    Bucket.Remove(this);
                }
            }

            public bool CompareAndSetState(int expected, int state)
            {
                return InterLockedEx.CompareAndSet(ref this.state, state, expected);
            }

            public int State
            {
                get { return state; }
            }

            public bool IsCancelled
            {
                get { return state == ST_CANCELLED; }
            }

            public bool IsExpired
            {
                get { return state > ST_IN_BUCKET; }
            }

            public void Expire()
            {
                if (!CompareAndSetState(ST_IN_BUCKET, ST_EXPIRED))
                {
                    //assert this.state != ST_INIT
                    return;
                }

                try
                {
                    _task.Run(this);
                }
                catch (Exception t)
                {
                    _timer.Logger.WriteWarning($"{typeof(ITimerTask).Name} 执行出错。", t);
                    t.ThrowIfNecessary();
                }
            }

            public void Dispose()
            {
                (this._task as IDisposable)?.Dispose();
                this.Prev?.Dispose();
                this.Next?.Dispose();
                this._task = null;
                this._timer = null;
                this.Bucket = null;
                this.Prev = null;
                this.Next = null;
            }
        }
    }
}
