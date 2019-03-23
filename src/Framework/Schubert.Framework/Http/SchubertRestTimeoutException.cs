using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class SchubertRestTimeoutException : SchubertRestException
    {
        public SchubertRestTimeoutException(string message)
            : base(HttpStatusCode.RequestTimeout, message)
        {

        }

        public SchubertRestTimeoutException(string message, Exception innerException)
            : base(HttpStatusCode.RequestTimeout, message, innerException)
        {

        }
    }
}
