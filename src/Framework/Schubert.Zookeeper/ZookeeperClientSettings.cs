using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Zookeeper
{
    /// <summary>
    /// ZooKeeper客户端选项。
    /// </summary>
    public class ZookeeperClientSettings
    {
        /// <summary>
        /// 创建一个新的ZooKeeper客户端选项。
        /// </summary>
        /// <remarks>
        /// <see cref="ConnectionTimeoutSeconds"/> 为10秒。
        /// <see cref="SessionTimeoutSeconds"/> 为20秒。
        /// <see cref="OperatingTimeoutSeconds"/> 为60秒。
        /// <see cref="ReadOnly"/> 为false。
        /// <see cref="SessionId"/> 为0。
        /// <see cref="SessionPasswd"/> 为null。
        /// <see cref="BasePath"/> 为null。
        /// <see cref="EnableEphemeralNodeRestore"/> 为true。
        /// </remarks>
        public ZookeeperClientSettings()
        {
        }

        /// <summary>
        /// 创建一个新的ZooKeeper客户端选项。
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> 为空。</exception>
        /// <remarks>
        /// <see cref="ConnectionTimeoutSeconds"/> 为10秒。
        /// <see cref="SessionTimeoutSeconds"/> 为20秒。
        /// <see cref="OperatingTimeoutSeconds"/> 为60秒。
        /// <see cref="ReadOnly"/> 为false。
        /// <see cref="SessionId"/> 为0。
        /// <see cref="SessionPasswd"/> 为null。
        /// <see cref="BasePath"/> 为null。
        /// <see cref="EnableEphemeralNodeRestore"/> 为true。
        /// </remarks>
        public ZookeeperClientSettings(string connectionString) : this()
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(paramName: nameof(connectionString), message: "Zookeeper 连接字符串不能为空。");

            ConnectionString = connectionString;
        }

        /// <summary>
        /// 获取或设置 Zookeeper 连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 获取或设置等待ZooKeeper连接的时间（默认为 10 秒）。
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// 获取或设置执行ZooKeeper操作的重试等待时间（默认为 60 秒）。
        /// </summary>
        public int OperatingTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// 获取或设置 zookeeper 会话超时时间（默认为 20 秒）。
        /// </summary>
        public int SessionTimeoutSeconds { get; set; } = 20;

        /// <summary>
        /// 获取或设置一个值，指示是否只读，默认为false。
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// 获取或设置会话Id。
        /// </summary>
        public long SessionId { get; set; }

        /// <summary>
        /// 获取或设置会话密码。
        /// </summary>
        public byte[] SessionPasswd { get; set; }

        /// <summary>
        /// 获取或设置基础路径，会在所有的zk操作节点路径上加入此基础路径。
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// 获取或设置一个值，指示是否启用短暂类型节点的恢复（默认为 true）。
        /// </summary>
        public bool EnableEphemeralNodeRestore { get; set; } = true;
    }
}
