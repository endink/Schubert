using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Caching
{
    /// <summary>
    /// 表示 Schubert 框架的 Redis 缓存配置。
    /// </summary>
    public class SchubertRedisOptions
    {
        private int _reconnectFrequencySeconds = 5;
        private int _retryDelayMs = 500;
        private int _retryCount = 1;
        /// <summary>
        /// 获取或设置 Redis 缓存连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 获取或设置是否对缓存对象使用 Gzip 压缩。
        /// </summary>
        public bool GZipCompress { get; set; }

        /// <summary>
        /// 获取或设置用来序列化缓存数据的序列化程序的名称（为空将默认使用 <see cref="RedisSerializerNames.JsonNet"/>，即 Json.Net 提供程序）。
        /// </summary>
        public string SerializerName { get; set; } = RedisSerializerNames.JsonNet;

        /// <summary>
        /// 获取或设置断线重连频率（分钟），最小值为 2, 默认为 5 秒。
        /// </summary>
        public int ReconnectFrequencySeconds
        {
            get { return _reconnectFrequencySeconds; }
            set { _reconnectFrequencySeconds = Math.Max(2, value); }
        }

        /// <summary>
        /// 在发生错误时候进行重试的次数，默认为 1。
        /// </summary>
        public int RetryCount
        {
            get { return _retryCount; }
            set { _retryDelayMs = Math.Max(0, value); }
        }

        /// <summary>
        /// 获取或设置断线重连频率（分钟），最小值为 500, 默认为 500。
        /// </summary>
        public int RetryDelayMilliseconds
        {
            get { return _retryDelayMs; }
            set { _retryDelayMs = Math.Max(500, value); }
        }
    }
}
