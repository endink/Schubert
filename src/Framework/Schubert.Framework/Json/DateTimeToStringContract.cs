using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Json
{
    public class DateTimeToStringContract : JsonPrimitiveContract
    {

        public DateTimeToStringContract(Type undType, string inputFormat, string outputFormat)
            :base(undType)
        {
            this.Converter = new DateTimeToStringJsonConverter(inputFormat, outputFormat);
        }
    }
}
