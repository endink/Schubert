using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class RestStreamedResponse : IDisposable
    {
        private HttpResponseMessage _message;

        public HttpStatusCode StatusCode { get; }

        public Stream Body { get; }

        public HttpResponseHeaders Headers { get; }

        public long? StreamLength { get; }

        public RestStreamedResponse(HttpResponseMessage message, Stream bodyStream)
        {
            this.StatusCode = message.StatusCode;
            this.Headers = message.Headers;
            this.Body = bodyStream;
            this.StreamLength = message.Content.Headers.ContentLength;
            _message = message;
        }

        public RestStreamedResponse(HttpStatusCode statusCode, Stream body, long? contentLength, HttpResponseHeaders headers)
        {
            this.StatusCode = statusCode;
            this.Body = body;
            this.Headers = headers;
            this.StreamLength = contentLength;
        }

        public void Dispose()
        {
            _message.Dispose();
            this.Body.Dispose();
        }
    }
}
