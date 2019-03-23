using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Configuration
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class JsonConfigurationParser
    {
        private IDictionary<string, string> _data;
        private Stack<string> _context;
        private string _currentPath;

        private JsonTextReader _reader;
        private IEnumerable<String> _excludedPaths;

        public IDictionary<string, string> Parse(String jsonContent, params String[] excludedPaths)
        {
            return this.Parse(jsonContent, excludedPaths as IEnumerable<String>);
        }

        public IDictionary<string, string> Parse(String jsonContent, IEnumerable<String> excludedPaths)
        {
            _excludedPaths = excludedPaths;
            _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _context = new Stack<string>();

            if (jsonContent.IsNullOrWhiteSpace())
            {
                return new Dictionary<String, String>(0);
            }
            using (StringReader stringReader = new StringReader(jsonContent))
            {
                using (_reader = new JsonTextReader(stringReader))
                {
                    _reader.DateParseHandling = DateParseHandling.None;

                    var jsonConfig = JObject.Load(_reader);

                    VisitJObject(jsonConfig);
                }
            }
            var data = _data;

            _excludedPaths = null;
            _reader = null;
            _data = null;
            _context = null;

            return data;
        }

        private void VisitJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                EnterContext(property.Name);
                if (_excludedPaths == null || !_excludedPaths.Contains(_currentPath, StringComparer.OrdinalIgnoreCase))
                {
                    VisitProperty(property);
                }
                ExitContext();
            }
        }

        private void VisitProperty(JProperty property)
        {
            VisitToken(property.Value);
        }

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token.Value<JValue>());
                    break;

                default:
                    throw new FormatException($"Unsupported JSON token '{_reader.TokenType}' was found. Path '{_reader.Path}', line {_reader.LineNumber} position {_reader.LinePosition}.");
            }
        }

        private void VisitArray(JArray array)
        {
            for (int index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(JValue data)
        {
            var key = _currentPath;

            if (_data.ContainsKey(key))
            {
                throw new FormatException(String.Format($"A duplicate key '{key}' was found."));
            }
            _data[key] = data.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }
    }

}
