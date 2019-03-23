using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class AnonymousCredentials : RestCredentials
    {
        public override bool IsTlsCredentials()
        {
            return false;
        }

        public override void Dispose()
        {
        }

        public override HttpMessageHandler GetHandler(HttpMessageHandler innerHandler)
        {
            return innerHandler;
        }
    }
}
