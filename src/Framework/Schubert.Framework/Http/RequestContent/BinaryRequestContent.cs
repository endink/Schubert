using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class BinaryRequestContent : IRequestContent
    {
        private readonly Stream _stream;
        private readonly string _mimeType;

        public BinaryRequestContent(Stream stream, string mimeType)
        {
            Guard.ArgumentNotNull(stream, nameof(stream));

            if (string.IsNullOrEmpty(mimeType))
            {
                throw new ArgumentNullException(nameof(mimeType));
            }

            this._stream = stream;
            this._mimeType = mimeType;
        }

        public HttpContent GetContent()
        {
            var data = new StreamContent(this._stream);
            data.Headers.ContentType = new MediaTypeHeaderValue(this._mimeType);
            return data;
        }
    }
}
