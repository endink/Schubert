using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Threading
{
    partial class HashedWheelTimer
    {
        private sealed class Worker : IDisposable
        {
            private ISet<ITimeout> unprocessedTimeouts;

            private long tick;
            private HashedWheelTimer _timer;
            private ManualResetEventSlim _waitHandler = null;

            public Worker(HashedWheelTimer timer)
            {
                unprocessedTimeouts = new HashSet<ITimeout>();
                _timer = timer;
                
                _waitHandler = new ManualResetEventSlim(false);
            }

            public void Stop()
            {
                if (!_waitHandler.IsSet)
                {
                    //中断 waitForNextTick 方法的 Wait。 
                    _waitHandler.Set();
                }
            }

            public void Run()
            {
                // Initialize the startTime.
                _timer.startTime = System.DateTime.UtcNow.Ticks;
                if (_timer.startTime == 0)
                {
                    // We use 0 as an indicator for the uninitialized value here, so make sure it's not 0 when initialized.
                    _timer.startTime = 1;
                }
                // Notify the other threads waiting for the initialization at start().
                _timer._startTimeInitialized.Set();

                _waitHandler.Reset();
                do
                {
                    long deadline = WaitForNextTick();
                    //运行被中断。
                    if (deadline > 0)
                    {
                        transferTimeoutsToBuckets();
                        HashedWheelBucket bucket =
                                _timer._wheel[(int)(tick & _timer._mask)];
                        bucket.ExpireTimeouts(deadline);
                        tick++;
                    }
                } while (Interlocked.Read(ref this._timer.workerState) == WORKER_STATE_STARTED);

                // Fill the unprocessedTimeouts so we can return them from stop() method.
                foreach (HashedWheelBucket bucket in _timer._wheel)
                {
                    bucket.ClearTimeouts(unprocessedTimeouts);
                }
                HashedWheelTimeout timeout = null;
                while (_timer._timeouts.TryDequeue(out timeout))
                {
                    unprocessedTimeouts.Add(timeout);
                }
            }

            private void transferTimeoutsToBuckets()
            {
                // transfer only max. 100000 timeouts per tick to prevent a thread to stale the workerThread when it just
                // adds new timeouts in a loop.
                for (int i = 0; i < 100000; i++)
                {

                    HashedWheelTimeout timeout = null;
                    if (!_timer._timeouts.TryDequeue(out timeout))
                    {
                        // all processed
                        break;
                    }
                    if (timeout.State == HashedWheelTimeout.ST_CANCELLED
                            ||  !timeout.CompareAndSetState(HashedWheelTimeout.ST_INIT, HashedWheelTimeout.ST_IN_BUCKET))
                    {
                        // Was cancelled in the meantime. So just remove it and continue with next HashedWheelTimeout
                        // in the queue
                        timeout.Remove();
                        continue;
                    }
                    long calculated = timeout.Deadline / this._timer._tickDurationTicks;
                    long remainingRounds = (calculated - tick) / this._timer._wheel.Length;
                    timeout.RemainingRounds = remainingRounds;

                    long ticks = Math.Max(calculated, tick); // Ensure we don't schedule for past.
                    int stopIndex = (int)(ticks & this._timer._mask);

                    HashedWheelBucket bucket = _timer._wheel[stopIndex];
                    bucket.addTimeout(timeout);
                }
            }
            /**
             * calculate goal nanoTime from startTime and current tick number,
             * then wait until that goal has been reached.
             * @return Long.MIN_VALUE if received a shutdown request,
             * current time otherwise (with Long.MIN_VALUE changed by +1)
             */
            private long WaitForNextTick()
            {
                long deadline = _timer._tickDurationTicks * (tick + 1);

                while(true)
                {
                    long currentTime = DateTime.UtcNow.Ticks - this._timer.startTime;
                    // ticks 单位为100纳秒，转换为毫秒 
                    long sleepTimeMs = (deadline - currentTime + 9999) / 10000;

                    if (sleepTimeMs <= 0)
                    {
                        if (currentTime == long.MaxValue)
                        {
                            return -long.MaxValue;
                        }
                        else
                        {
                            return currentTime;
                        }
                    }

                    //收到停止信号我们认为总是成功的。
                    if (_waitHandler.Wait(TimeSpan.FromMilliseconds(sleepTimeMs)))
                    {
                        if (Interlocked.Read(ref this._timer.workerState) == WORKER_STATE_SHUTDOWN)
                        {
                            return long.MinValue;
                        }
                    }
                }
            }

            public IEnumerable<ITimeout> UnprocessedTimeouts()
            {
                return unprocessedTimeouts?.ToArray() ?? Enumerable.Empty<ITimeout>();
            }

            public void Dispose()
            {
                _waitHandler?.Dispose();
                _waitHandler = null;
                _timer = null;
            }
        }
    }
}
