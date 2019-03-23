using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class SchubertRestException : SchubertException
    {
        public HttpStatusCode HttpCode { get; set; }

        public SchubertRestException(HttpStatusCode code)
        {
        }
        public SchubertRestException(HttpStatusCode code, string message)
            : base(message)
        {
            this.HttpCode = code;
        }
        
        public SchubertRestException(HttpStatusCode code, string message, Exception innerException)
            : base(message, innerException)
        {
            this.HttpCode = code;
        }
    }
}
