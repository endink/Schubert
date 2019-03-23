using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Json
{
    public class LongToStringContract : JsonPrimitiveContract
    {
        public LongToStringContract(Type underlyingType) : base(underlyingType)
        {
            this.Converter = new LongToStringJsonConverter();
        }   
    }
}
