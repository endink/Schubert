using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Caching
{
    public class JsonNetSerializer : IRedisCacheSerializer
    {
        private JsonSerializer _serializer = null;
        public JsonNetSerializer()
        {
            _serializer = new JsonSerializer();
            _serializer.NullValueHandling = NullValueHandling.Ignore;
            _serializer.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            _serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            _serializer.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
            _serializer.TypeNameHandling = TypeNameHandling.Objects;
        }

        public string Name
        {
            get
            {
                return RedisSerializerNames.JsonNet;
            }
        }

        public object Deserialize(TextReader reader, Type objectType)
        {
            return _serializer.Deserialize(reader, objectType);
        }


        public void Serialize(TextWriter textWriter, object value)
        {
            _serializer.Serialize(textWriter, value);
        }
    }
}
