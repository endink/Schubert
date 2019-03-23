using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class RestResponse
    {
        public HttpStatusCode StatusCode { get; }

        public string Body { get; }

        public HttpResponseHeaders Headers { get; }

        public RestResponse(HttpStatusCode statusCode, string body ,HttpResponseHeaders header)
        {
            this.StatusCode = statusCode;
            this.Body = body;
            this.Headers = header;
        }
    }
}
