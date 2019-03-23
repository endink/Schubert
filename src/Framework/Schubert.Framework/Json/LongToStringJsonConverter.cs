using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Json
{
    public class LongToStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (typeof(long) == objectType || typeof(long?) == objectType)
                return true;
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //throw new NotImplementedException();
            if (reader.Value == null)
            {
                if (!objectType.Equals(typeof(long?)))
                {
                    throw new JsonSerializationException($"JSON 内容 '{reader.Path}' 中的值为 null, 但是此处需要的值类型为 '{objectType.Name}'，不是可空类型。");
                }
                return null;
            }

            try
            {
                return long.Parse(reader.Value.ToString());
            }
            catch (FormatException ex)
            {
                throw new JsonSerializationException($"无法将 '{reader.Value.ToString()}' 文本转换为 '{objectType.Name}' 类型 (json path : {reader.Path})。", ex);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
}
