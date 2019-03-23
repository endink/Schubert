using Swifty.MicroServices.Client;
using Swifty.MicroServices.Server;
using System;

namespace Schubert.Framework.Swifty
{
    /// <summary>
    /// Swifty 配置选项。
    /// </summary>
    public class SwiftyOptions
    {
        private ExtendedSwiftyClientOptions _client;
        private ExtendedSwiftyServerOptions _server;
        /// <summary>
        /// 获取或设置客户端配置。
        /// </summary>
        public ExtendedSwiftyClientOptions Client
        {
            get { return _client ?? (_client = new ExtendedSwiftyClientOptions()); }
            set { _client = value; }
        }

        /// <summary>
        /// 获取或设置服务端配置。
        /// </summary>
        public ExtendedSwiftyServerOptions Server
        {
            get { return _server ?? (_server = new ExtendedSwiftyServerOptions()); }
            set { _server = value; }
        }

        /// <summary>
        /// 获取或设置要启用的 Swifty 功能, 默认为 <see cref="SwiftyFeatures.Both"/>。
        /// </summary>
        public SwiftyFeatures EnableFeatures { get; set; } = SwiftyFeatures.Both;

    }

    /// <summary>
    /// 指示 Swifty 功能。
    /// </summary>
    [Flags]
    public enum SwiftyFeatures
    {
        /// <summary>
        /// Swifty 服务端功能。
        /// </summary>
        Server = 1,
        /// <summary>
        /// Swifty 客户端功能。
        /// </summary>
        Client = 2,

        /// <summary>
        /// 同时启用服务端和客户端功能。
        /// </summary>
        Both = Server | Client
    }
}
