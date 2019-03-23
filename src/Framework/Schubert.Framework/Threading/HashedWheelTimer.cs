using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Threading
{
    /// <summary>
    /// 一个基于时间轮算法的定时器。
    /// 参考JAVA Netty HashedWheelTimer 类实现。
    /// </summary>
    public partial class HashedWheelTimer : ITimeoutTimer
    {
        private static int id;

        private static readonly Object MisuseDetectorSync = new Object();

        private static MisuseDetector misuseDetector = null;

        //private static AtomicIntegerFieldUpdater<HashedWheelTimer> WORKER_STATE_UPDATER =
        //    AtomicIntegerFieldUpdater.newUpdater(HashedWheelTimer.class, "workerState");

        private Worker _worker;
        private Thread _workerThread;

        public const long WORKER_STATE_INIT = 0;
        public const long WORKER_STATE_STARTED = 1;
        public const long WORKER_STATE_SHUTDOWN = 2;
        private long workerState = WORKER_STATE_INIT; // 0 - init, 1 - started, 2 - shut down

        private readonly long _tickDurationTicks;
        private HashedWheelBucket[] _wheel;
        private readonly int _mask;
        private ManualResetEventSlim _startTimeInitialized;
        private readonly ConcurrentQueue<HashedWheelTimeout> _timeouts;
        private long startTime;

        /// <summary>
        /// 创建 <see cref="HashedWheelTimer"/> 类的新实例。
        /// </summary>
        /// <param name="tickDuration">刻度（时间轮上的每个刻度表示的时间间隔）。</param>
        /// <param name="ticksPerWheel">表示时间轮上一圈有多少个刻度。</param>
        /// <param name="logger">日志记录器。</param>
        public HashedWheelTimer(TimeSpan tickDuration, int ticksPerWheel, ILogger logger)
        {
            if (tickDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("tickDuration 必须大于 0");
            }
            if (ticksPerWheel <= 0)
            {
                throw new ArgumentOutOfRangeException("ticksPerWheel 必须大于 0。");
            }
            this._timeouts = new ConcurrentQueue<HashedWheelTimeout>();
            this._worker = new Worker(this);
            this.Logger = logger ?? NullLogger.Instance;
            InitializeMisuseDetector();
            _startTimeInitialized = new ManualResetEventSlim(false);
            // Normalize ticksPerWheel to power of two and initialize the wheel.
            _wheel = CreateWheel(ticksPerWheel);
            _mask = _wheel.Length - 1;

            // Convert tickDuration to nanos.
            this._tickDurationTicks = tickDuration.Ticks;

            // Prevent overflow.
            if (this._tickDurationTicks >= long.MaxValue / _wheel.Length)
            {
                throw new ArgumentOutOfRangeException(
                        $"tickDuration 值 {tickDuration} 不符合预期 （tickDuration 换算为纳秒单位应该满足: 0 < {tickDuration} < {long.MaxValue / _wheel.Length}）");
            }

            _workerThread = new Thread(_worker.Run);
            _workerThread.Name = $"Hashed wheel timer #{Interlocked.Increment(ref id)}";
            //workerThread.Start();

            // Misuse check
            misuseDetector.Increase();
        }

        internal ILogger Logger { get; }

        private void InitializeMisuseDetector()
        {
            if (misuseDetector == null)
            {
                lock (MisuseDetectorSync)
                {
                    if (misuseDetector == null)
                    {
                        misuseDetector = new MisuseDetector(typeof(HashedWheelTimer), this.Logger, 256);
                    }
                }
            }
        }

        /// <summary>
        /// 创建时间轮刻度上的桶。
        /// </summary>
        private static HashedWheelBucket[] CreateWheel(int ticksPerWheel)
        {
            if (ticksPerWheel <= 0)
            {
                throw new ArgumentOutOfRangeException(
                        "ticksPerWheel 必须大于 0，实际值：" + ticksPerWheel);
            }
            if (ticksPerWheel > 1073741824)
            {
                throw new ArgumentOutOfRangeException(
                        "ticksPerWheel 不能大于 2^30，实际值：" + ticksPerWheel);
            }

            ticksPerWheel = NormalizeTicksPerWheel(ticksPerWheel);
            HashedWheelBucket[] wheel = new HashedWheelBucket[ticksPerWheel];
            for (int i = 0; i < wheel.Length; i++)
            {
                wheel[i] = new HashedWheelBucket();
            }
            return wheel;
        }

        private static int NormalizeTicksPerWheel(int ticksPerWheel)
        {
            int normalizedTicksPerWheel = 1;
            while (normalizedTicksPerWheel < ticksPerWheel)
            {
                normalizedTicksPerWheel <<= 1;
            }
            return normalizedTicksPerWheel;
        }

        public void Start()
        {
            switch (Interlocked.Read(ref this.workerState))
            {
                case WORKER_STATE_INIT:
                    if (InterLockedEx.CompareAndSet(ref this.workerState, WORKER_STATE_STARTED, WORKER_STATE_INIT))
                    {
                        _workerThread.Start();
                    }
                    break;
                case WORKER_STATE_STARTED:
                    break;
                case WORKER_STATE_SHUTDOWN:
                    throw new HashedWheelTimerException("HashedWheelTimer 不支持的停止以后再次开启。");
                default:
                    throw new HashedWheelTimerException("HashedWheelTimer 状态错误。");
            }

            // Wait until the startTime is initialized by the worker.
            while (startTime == 0)
            {
                _startTimeInitialized.Wait();
            }
        }

        public IEnumerable<ITimeout> Stop()
        {
            if (Thread.CurrentThread == _workerThread)
            {
                throw new InvalidOperationException(
                            $"{nameof(HashedWheelTimer)}.{nameof(HashedWheelTimer.Stop)} 不能在 worker 线程被调用。");
            }

            if (!InterLockedEx.CompareAndSet(ref this.workerState, WORKER_STATE_SHUTDOWN, WORKER_STATE_STARTED))
            {
                // workerState can be 0 or 2 at this moment - let it always be 2.
                Interlocked.Exchange(ref this.workerState, WORKER_STATE_SHUTDOWN);

                misuseDetector.Decrease();

                return Enumerable.Empty<ITimeout>();
            }
            //等待线程处理完成。
            while (_workerThread.IsAlive)
            {
                _worker.Stop();
                _workerThread.Join(100);
            }

            misuseDetector.Decrease();
            return _worker.UnprocessedTimeouts();
        }

        public ITimeout NewTimeout(ITimerTask task, TimeSpan delay)
        {
            Guard.ArgumentNotNull(task, nameof(task));


            this.Start();
            var now = DateTime.UtcNow.Ticks;
            // 添加任务到下一个时间刻度。
            // 任务被处理后会被装进 HashedWheelBucket 桶。
            long deadline = now + (long)(delay.TotalMilliseconds * 10000L) - startTime;
            HashedWheelTimeout timeout = new HashedWheelTimeout(this, task, deadline);
            _timeouts.Enqueue(timeout);
            return timeout;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();
                this._startTimeInitialized?.Dispose();
                this._worker?.Dispose();

                this._worker = null;
                this._startTimeInitialized = null;
                if (this._wheel != null)
                {
                    var wheel = this._wheel;
                    this._wheel = null;
                    foreach (var b in wheel)
                    {
                        b.Dispose();
                    }
                }
            }
        }

        ~HashedWheelTimer()
        {
            this.Dispose(false);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}
