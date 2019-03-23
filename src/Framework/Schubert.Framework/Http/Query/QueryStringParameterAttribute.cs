using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http.Query
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =false, Inherited = true)]
    public class QueryStringParameterAttribute : Attribute
    {
        public string Name { get; private set; }

        public bool IsRequired { get; private set; }

        public Type ConverterType { get; private set; }

        public QueryStringParameterAttribute(string name, bool required = false, Type converterType = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (converterType != null && !converterType.GetTypeInfo().GetInterfaces().Contains(typeof(IQueryStringConverter)))
            {
                throw new ArgumentException($"Provided query string converter type is not {typeof(IQueryStringConverter).FullName}", nameof(converterType));
            }

            this.Name = name;
            this.IsRequired = required;
            this.ConverterType = converterType;
        }
    }
}
