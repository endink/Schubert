using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http.Query
{
    public class QueryString
    {
        private Object Object { get; }

        private Tuple<PropertyInfo, QueryStringParameterAttribute>[] AttributedPublicProperties { get; }

        private IQueryStringConverterFactory QueryStringConverterFactory { get; }

        public QueryString(Object value)
        {
            Guard.ArgumentNotNull(value, nameof(value));

            this.Object = value;
            this.QueryStringConverterFactory = new QueryStringConverterFactory();
            this.AttributedPublicProperties = FindAttributedPublicProperties<QueryStringParameterAttribute>(value.GetType());
        }

        private IDictionary<string, string[]> GetKeyValuePairs()
        {
            var queryParameters = new Dictionary<string, string[]>();
            foreach (var pair in this.AttributedPublicProperties)
            {
                var property = pair.Item1;
                var attribute = pair.Item2;
                var value = property.GetValue(this.Object, null);

                // 'Required' check
                if (attribute.IsRequired && value == null)
                {
                    string propertyFullName = $"{property.GetType().FullName}.{property.Name}";
                    throw new ArgumentException("Got null/unset value for a required query parameter.", propertyFullName);
                }

                // Serialize
                if (attribute.IsRequired || !IsDefaultOfType(value))
                {
                    var keyStr = attribute.Name;
                    string[] valueStr;
                    if (attribute.ConverterType == null)
                    {
                        valueStr = new[] { value.ToString() };
                    }
                    else
                    {
                        var converter = this.QueryStringConverterFactory.GetConverterInstance(attribute.ConverterType);
                        valueStr = this.ConvertValue(converter, value);

                        if (valueStr == null)
                        {
                            throw new InvalidOperationException($"Got null from value converter '{attribute.ConverterType.FullName}'");
                        }
                    }

                    queryParameters[keyStr] = valueStr;
                }
            }

            return queryParameters;
        }

        /// <summary>
        /// Returns formatted query string.
        /// </summary>
        /// <returns></returns>
        private string GetQueryString(bool encoding = true)
        {
            return string.Join("&",
                GetKeyValuePairs().Select(
                    pair => string.Join("&",
                        pair.Value.Select(
                            v => $"{(encoding ? Uri.EscapeUriString(pair.Key): pair.Key)}={(encoding ? Uri.EscapeDataString(v) : v)}"))));
        }

        public String ToString(bool encoding)
        {
            return this.GetQueryString(encoding);
        }

        public override string ToString()
        {
            return this.GetQueryString();
        }

        private string[] ConvertValue(IQueryStringConverter converter, object value)
        {
            if (!converter.CanConvert(value.GetType()))
            {
                throw new InvalidOperationException(
                    $"Cannot convert type {value.GetType().FullName} using {converter.GetType().FullName}.");
            }
            return converter.Convert(value);
        }

        private Tuple<PropertyInfo, TAttribType>[] FindAttributedPublicProperties<TAttribType>(Type t) 
            where TAttribType : Attribute
        {
            var ofAttributeType = typeof(TAttribType);

            var properties = t.GetTypeInfo().GetProperties();
            var publicProperties = properties.Where(p => p.GetGetMethod(false).IsPublic);
            if (!publicProperties.Any())
            {
                throw new InvalidOperationException($"No public property getters found on type {t.FullName}.");
            }

            var attributedPublicProperties = properties.Where(p => p.GetCustomAttribute<TAttribType>() != null).ToArray();
            if (!attributedPublicProperties.Any())
            {
                throw new InvalidOperationException(
                    $"No public properties attributed with [{ofAttributeType.FullName}] found on type {t.FullName}.");
            }

            return attributedPublicProperties.Select(pi =>
                new Tuple<PropertyInfo, TAttribType>(pi, pi.GetCustomAttribute<TAttribType>())).ToArray();
        }

        private static bool IsDefaultOfType(object o)
        {
            if (o is ValueType)
            {
                return o.Equals(Activator.CreateInstance(o.GetType()));
            }

            return o == null;
        }
        

        public static implicit operator String(QueryString m)
        {
            return m.GetQueryString();
        }
    }

    /// <summary>
    /// Generates query string formatted as:  [url]?key=value1&amp;key=value2&amp;key=value3...
    /// </summary>
    public class EnumerableQueryString
    {
        private readonly string _key;
        private readonly string[] _data;

        public EnumerableQueryString(string key, string[] data)
        {
            _key = key;
            _data = data;
        }

        /// <summary>
        /// Returns formatted query string.
        /// </summary>
        /// <returns></returns>
        private string GetQueryString()
        {
            return string.Join("&",
                        _data.Select(
                            v => $"{Uri.EscapeUriString(_key)}={Uri.EscapeDataString(v)}"));
        }

        public override string ToString()
        {
            return this.GetQueryString();
        }

        public static implicit operator String(EnumerableQueryString m)
        {
            return m.GetQueryString();
        }
    }
}
