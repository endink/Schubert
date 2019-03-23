using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    /// <summary>
    /// twitter 分布式  Id 生成算法（当前配置精度为秒，支持每秒生成 4095 个 Id，支持 1023 个机器ID，支持 17 个数据中心）。
    /// ID 生成长度 16（前30年）-17（30年后）位，起始时间 2015-01-01。
    /// 通常机器ID按网段划分可支持4个网段。
    /// </summary>
    public class SnowflakeWorker
    {

        public static readonly DateTime Jan1st1970 = new DateTime
           (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        //2015-01-01
        public const long Twepoch = 1430438400L;

        const int WorkerIdBits = 10;
        const int DatacenterIdBits = 4;
        const int SequenceBits = 12;
        const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
        const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);

        private const int WorkerIdShift = SequenceBits;
        private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
        public const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;
        private const long SequenceMask = -1L ^ (-1L << SequenceBits);

        private long _sequence = 0L;
        private long _lastTimestamp = -1L;


        public SnowflakeWorker(long workerId, long datacenterId, long sequence = 0L)
        {   
            // sanity check for workerId
            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException(String.Format("worker Id can't be greater than {0} or less than 0", MaxWorkerId));
            }

            if (datacenterId > MaxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException(String.Format("datacenter Id can't be greater than {0} or less than 0", MaxDatacenterId));
            }

            WorkerId = workerId;
            DatacenterId = datacenterId;
            _sequence = sequence;

         

            //log.info(
            //    String.Format("worker starting. timestamp left shift {0}, datacenter id bits {1}, worker id bits {2}, sequence bits {3}, workerid {4}",
            //                  TimestampLeftShift, DatacenterIdBits, WorkerIdBits, SequenceBits, workerId)
            //    );	
        }

        public long WorkerId { get; protected set; }
        public long DatacenterId { get; protected set; }

        public long Sequence
        {
            get { return _sequence; }
            internal set { _sequence = value; }
        }

        // def get_timestamp() = System.currentTimeMillis

        readonly object _lock = new Object();

        public virtual long NextId()
        {
            lock (_lock)
            {
                var timestamp = InternalCurrentTime();

                if (timestamp < _lastTimestamp)
                {
                    //exceptionCounter.incr(1);
                    //log.Error("clock is moving backwards.  Rejecting requests until %d.", _lastTimestamp);
                    throw new InvalidOperationException(String.Format(
                        "Clock moved backwards.  Refusing to generate id for {0} milliseconds", _lastTimestamp - timestamp));
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & SequenceMask;
                    if (_sequence == 0)
                    {
                        timestamp = TilNextTime(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;
                var id = ((timestamp - Twepoch) << TimestampLeftShift) |
                         (DatacenterId << DatacenterIdShift) |
                         (WorkerId << WorkerIdShift) | _sequence;

                return id;
            }
        }

        protected virtual long TilNextTime(long lastTimestamp)
        {
            var timestamp = InternalCurrentTime();
            while (timestamp <= lastTimestamp)
            {
                timestamp = InternalCurrentTime();
            }
            return timestamp;
        }


        private static long InternalCurrentTime()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalSeconds;
        }
    }
}
