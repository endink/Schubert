using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Authentication
{
    public static class JObjectExtensions
    {
        public static string GetValueOrDefault(this JObject user, string propertyName, string subProperty)
        {
            JToken jToken;
            if (user.TryGetValue(propertyName, out jToken))
            {
                JObject jObject = JObject.Parse(jToken.ToString());
                if (jObject != null && jObject.TryGetValue(subProperty, out jToken))
                {
                    return jToken.ToString();
                }
            }
            return null;
        }

        public static string GetValueOrDefault(this JObject user, string propertyName)
        {
            JToken jToken;
            if (user.TryGetValue(propertyName, out jToken))
            {
                return jToken.ToString();
            }
            return null;
        }
    }
}
