using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Schubert.Framework.Web
{
    /// <summary>
    /// 表示 Web 客户端环境。
    /// </summary>
    public interface IClientEnvironment
    {
        /// <summary>
        /// 获取客户端的 X509 证书（证书读取操作在同一个上下文中第一次获取会花费一定时间，如果不存在，返回 null）。
        /// </summary>
        X509Certificate2 ClientCertificate { get; }

        /// <summary>
        /// 获取客户端的 IP 地址。
        /// </summary>
        IPAddress IpAddress { get; set; }

        /// <summary>
        /// 获取客户端的端口号。
        /// </summary>
        int Port { get; set; }
    }
}