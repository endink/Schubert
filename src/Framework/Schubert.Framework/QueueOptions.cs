using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    public class QueueOptions
    {
        private int _transferConcurrencyLevel;

        public QueueOptions()
        {
            this._transferConcurrencyLevel = 1;
            this.QueueMaxCount = 300000;
            this.QueueMaxSizeKBytes = 500 * 1024;
        }

        /// <summary>
        /// 获取或设置本地缓存队列允许的大小（默认为 500 * 1024, 即 500M）。
        /// </summary>
        public int QueueMaxSizeKBytes { get; set; }

        /// <summary>
        /// 获取或设置队列允许的最大消息条数（默认为 300000）。
        /// </summary>
        public int QueueMaxCount { get; set; }

        /// <summary>
        /// 表示消费的并发度（默认为 1，表示单线程）。
        /// </summary>
        public int ConsumeConcurrencyLevel
        {
            get { return Math.Max(1, _transferConcurrencyLevel); }
            set { _transferConcurrencyLevel = value; }
        }
    }
}
