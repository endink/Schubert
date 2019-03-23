using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http.Query
{
    public class JsonQueryStringConverter : IQueryStringConverter
    {
        public bool CanConvert(Type t)
        {
            return true;
        }

        public string[] Convert(object o)
        {
            if (o == null)
            {
                return new String[0];
            }
            return new[] { JsonConvert.SerializeObject(o, Formatting.None) };
        }
    }
}
