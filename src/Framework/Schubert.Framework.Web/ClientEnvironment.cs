using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    /// <summary>
    /// 表示 Web 客户端的环境。
    /// </summary>
    public class ClientEnvironment : IClientEnvironment
    {
        private ITlsConnectionFeature _clientCertificateFeature;
        private X509Certificate2 _x509Cretificate = null;
        public ClientEnvironment(IHttpContextAccessor httpContextAccessor)
        {
            Guard.ArgumentNotNull(httpContextAccessor, nameof(httpContextAccessor));
            
            var connectionFeature = httpContextAccessor.HttpContext.Features.Get<IHttpConnectionFeature>();
            _clientCertificateFeature = httpContextAccessor.HttpContext.Features.Get<ITlsConnectionFeature>();
            if (connectionFeature != null)
            {
                this.IpAddress = connectionFeature.RemoteIpAddress;
                this.Port = connectionFeature.RemotePort;
            }
        }
        public IPAddress IpAddress { get; set; }
        public int Port { get; set; }

        public X509Certificate2 ClientCertificate
        {
            get { return _x509Cretificate ?? (_x509Cretificate = _clientCertificateFeature?.ClientCertificate); }
        } 
    }
}
