using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Schubert.Framework.Http.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class HttpJsonSerializer
    {
        public JsonSerializerSettings Settings { get; set; }

        static HttpJsonSerializer()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public HttpJsonSerializer(JsonSerializerSettings setting)
        {
            this.Settings = setting ?? new JsonSerializerSettings();
        }

        public HttpJsonSerializer(params JsonConverter[] converters)
        {
            this.Settings = new JsonSerializerSettings();
            this.Settings.NullValueHandling = NullValueHandling.Ignore;
            this.Settings.Converters.AddRange(converters ?? new JsonConverter[0]);
        }

        public HttpJsonSerializer(IEnumerable<JsonConverter> converters)
            :this(converters?.ToArray())
        {
        }

        public JObject DeserializeJsonObject(string json)
        {
            return JObject.Parse(json);
        }

        public T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, this.Settings);
        }

        public string SerializeObject<T>(T value)
        {
            return JsonConvert.SerializeObject(value, this.Settings);
        }
    }
}
