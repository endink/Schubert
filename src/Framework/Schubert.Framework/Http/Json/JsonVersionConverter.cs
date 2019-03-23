using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http.Json
{
    public class JsonVersionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else if (value is Version)
            {
                writer.WriteValue(value.ToString());
            }
            else
            {
                throw new InvalidOperationException($"Cannot seserialize value of type '{value.GetType().Name}' to '{typeof(Version).Name}' ");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var strVal = reader.Value as string;
            //if (strVal == null)
            //{
            //    var valueType = reader.Value == null ? "<null>" : reader.Value.GetType().FullName;
            //    throw new InvalidOperationException($"Cannot deserialize value of type '{valueType}' to '{objectType.FullName}' ");
            //}
            if (strVal.IsNullOrWhiteSpace())
            {
                return null;
            }
            return Version.Parse(strVal);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Version);
        }
    }
}
