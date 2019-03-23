using Microsoft.Net.Http.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class CertificaterRestCredentials : RestCredentials
    {
        private readonly X509Certificate2 _certificate;

        public CertificaterRestCredentials(X509Certificate2 clientCertificate)
        {
            _certificate = clientCertificate;
        }
        
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; }

        public override HttpMessageHandler GetHandler(HttpMessageHandler innerHandler)
        {
            var handler = (ManagedHandler)innerHandler;
            handler.ClientCertificates = new X509CertificateCollection
            {
                _certificate
            };

            handler.ServerCertificateValidationCallback = this.ServerCertificateValidationCallback;
#if NETFX
            if (handler.ServerCertificateValidationCallback == null)
            {
                handler.ServerCertificateValidationCallback = ServicePointManager.ServerCertificateValidationCallback;
            }
#endif

            return handler;
        }

        public override bool IsTlsCredentials()
        {
            return true;
        }

        public override void Dispose()
        {
            _certificate.Dispose();
        }
    }
}
