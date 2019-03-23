using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public abstract class RestCredentials : IDisposable
    {
        public abstract bool IsTlsCredentials();

        public abstract HttpMessageHandler GetHandler(HttpMessageHandler innerHandler);

        public virtual void Dispose()
        {
        }
    }
}
