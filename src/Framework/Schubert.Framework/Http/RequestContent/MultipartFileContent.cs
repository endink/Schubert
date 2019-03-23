using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Schubert.Framework.Http
{
    public class MultipartFileContent : IRequestContent
    {
        private MultipartFormDataContent _content;

        public MultipartFileContent(string boundary = null)
        {
            _content = boundary.IsNullOrWhiteSpace() ? new MultipartFormDataContent() : new MultipartFormDataContent(boundary.Trim());
        }

        public void AddFile(string fileName, Stream fileStream)
        {
            Guard.ArgumentNullOrWhiteSpaceString(fileName, nameof(fileName));
            Guard.ArgumentNotNull(fileStream, nameof(fileStream));

            _content.Add(new StreamContent(fileStream), Path.GetFileNameWithoutExtension(fileName), fileName);
        }

        public HttpContent GetContent()
        {
            return _content;
        }
    }
}
