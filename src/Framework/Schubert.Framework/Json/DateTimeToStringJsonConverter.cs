using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Json
{
    public class DateTimeToStringJsonConverter : JsonConverter
    {
        private String _inputFormat = null;
        private String _outputFormat = null;

        public DateTimeToStringJsonConverter(String inputDateFormat = null, String outputDateFormat = null)
        {
            _inputFormat = inputDateFormat;
            _outputFormat = outputDateFormat;
        }

        public override bool CanConvert(Type objectType)
        {
            if (typeof(DateTime) == objectType || typeof(DateTime?) == objectType || typeof(DateTimeOffset) == objectType || typeof(DateTimeOffset?) == objectType)
            {
                return true;
            }
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //throw new NotImplementedException();
            if (reader.Value == null)
            {
                if (!objectType.Equals(typeof(DateTime?)) && !objectType.Equals(typeof(DateTimeOffset?)))
                {
                    throw new JsonSerializationException($"JSON 内容 '{reader.Path}' 中的值为 null, 但是此处需要的值类型为 '{objectType.Name}'，不是可空类型。");
                }
                return null;
            }

            try
            {
                var date = _inputFormat.IsNullOrWhiteSpace() ? DateTime.Parse(reader.Value.ToString()) : DateTime.ParseExact(reader.Value.ToString(), _inputFormat, CultureInfo.InvariantCulture);
                if (objectType.Equals(typeof(DateTime)) || objectType.Equals(typeof(DateTime?)))
                {
                    return date;
                }
                return new DateTimeOffset(date);
            }
            catch (FormatException ex)
            {
                throw new JsonSerializationException($"无法将 '{reader.Value.ToString()}' 文本转换为 '{objectType.Name}' 类型 (json path : {reader.Path})。", ex);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteValue((String)null);
            }
            if (value.GetType().Equals(typeof(DateTime)))
            {
                string stringValue = ((DateTime)value).ToString(_outputFormat.IfNullOrWhiteSpace("yyyy-MM-dd HH:mm:ss"));
                writer.WriteValue(stringValue);
            }
            if (value.GetType().Equals(typeof(DateTime?)))
            {
                string stringValue = ((DateTime?)value)?.ToString(_outputFormat.IfNullOrWhiteSpace("yyyy-MM-dd HH:mm:ss"));
                writer.WriteValue(stringValue);
            }
            if (value.GetType().Equals(typeof(DateTimeOffset)))
            {
                string stringValue = ((DateTimeOffset)value).ToString(_outputFormat.IfNullOrWhiteSpace("yyyy-MM-dd HH:mm:ss"));
                writer.WriteValue(stringValue);
            }
            if (value.GetType().Equals(typeof(DateTimeOffset?)))
            {
                string stringValue = ((DateTimeOffset?)value)?.ToString(_outputFormat.IfNullOrWhiteSpace("yyyy-MM-dd HH:mm:ss"));
                writer.WriteValue(stringValue);
            }
        }
    }
}
