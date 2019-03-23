using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class JsonRequestContent : IRequestContent
    {
        private const string JsonMimeType = "application/json";

        private readonly Object _value;
        private readonly HttpJsonSerializer _serializer;

        public JsonRequestContent(Object val, HttpJsonSerializer serializer)
        {
            Guard.ArgumentNotNull(val, nameof(val));
            Guard.ArgumentNotNull(serializer, nameof(serializer));

            this._value = val;
            this._serializer = serializer;
        }

        public HttpContent GetContent()
        {
            var serializedObject = this._serializer.SerializeObject(this._value);
            return new StringContent(serializedObject, Encoding.UTF8, JsonMimeType);
        }
    }
}
