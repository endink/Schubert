using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class PhysicalFileStorageProvider : IFileStorageProvider
    {
        private PhysicalFileStorageOptions _options;
        public const string DefaultScope = "__default";
        private IEnumerable<String> _patterns = null;

        public PhysicalFileStorageProvider(PhysicalFileStorageOptions options)
        {
            Guard.ArgumentNotNull(options, nameof(options));
            this._options = options;
            this._patterns = GetKeyPatterns();
        }

        private IEnumerable<String> GetKeyPatterns()
        {
            return _options.IncludeScopes.Select(s => 
            {
                String pattern = String.Empty;
                for (int i = 0; i < s.Length; i++)
                {
                    string c = s.Substring(i, 1);
                    if (c == "?")
                    {
                        pattern = String.Concat(pattern, @"(\S)");
                    }
                    else if (c == "*")
                    {
                        pattern = String.Concat(pattern, @"(\S)*");
                    }
                    else
                    {
                        pattern = String.Concat(pattern, c.EscapeForRegex());
                    }
                }
                return pattern;
            });
        }

        public IFileStorage CreateStorage(string scope = null)
        {
            scope = scope.IfNullOrWhiteSpace(DefaultScope);
            var supported = _patterns.Any(p => Regex.IsMatch(scope, p, RegexOptions.IgnoreCase));
            return new PhysicalFileStorage(scope, _options.FileMapping);
        }
    }
}
