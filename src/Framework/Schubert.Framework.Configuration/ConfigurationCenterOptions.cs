using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Zookeeper;

namespace Schubert.Framework.Configuration
{
    /// <summary>
    /// 表示配置中心的选项。
    /// </summary>
    public class ConfigurationCenterOptions
    {
        private ZookeeperClientSettings _zookeeperSettings = null;
        private string _nodeBasePath = null;

        /// <summary>
        /// 获取配置中心配置节点路径（需要根据配置中心配置，不建议修改）。
        /// </summary>
        public string NodeBasePath
        {
            get { return _nodeBasePath.IfNullOrWhiteSpace("central_config_encrypt/{env}/{group}/{appname}/{version}"); }
            set { _nodeBasePath = value; }
        }

        /// <summary>
        /// 获取或设置服务器连接字符串（默认实现为 Zookeeper，使用 Zookeeper 连接字符串）。
        /// </summary>
        public string ConnectionString
        {
            get { return this.Zookeeper.ConnectionString; }
            set { this.Zookeeper.ConnectionString = value; }
        }

        /// <summary>
        /// 获取或设置配置中心连接超时时间（默认为 10 秒）。
        /// </summary>
        public int ConnectionTimeoutSeconds
        {
            get { return this.Zookeeper.ConnectionTimeoutSeconds; }
            set { this.Zookeeper.ConnectionTimeoutSeconds = value; }
        }

        /// <summary>
        /// 获取或设置配置中心操作超时时间（默认为 60 秒）
        /// </summary>
        public int OperatingTimeoutSeconds
        {
            get { return this.Zookeeper.OperatingTimeoutSeconds; }
            set { this.Zookeeper.OperatingTimeoutSeconds = value; }
        }

        /// <summary>
        /// 获取或设置配置中心会话超时时间（默认为 20 秒）
        /// </summary>
        public int SessionTimeoutSeconds
        {
            get { return this.Zookeeper.SessionTimeoutSeconds; }
            set { this.Zookeeper.SessionTimeoutSeconds = value; }
        }

        /// <summary>
        /// 获取或设置配置中心依赖的 Zookeeper 配置。
        /// </summary>
        internal ZookeeperClientSettings Zookeeper
        {
            get { return _zookeeperSettings ?? (_zookeeperSettings = new ZookeeperClientSettings() { ReadOnly = true }); }
        }
    }
}
