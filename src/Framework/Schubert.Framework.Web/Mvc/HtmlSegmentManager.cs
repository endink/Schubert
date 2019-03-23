using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc
{
    internal class HtmlSegmentManager : IHtmlSegmentManager
    {
        private readonly Dictionary<String, StringBuilder> _scripts;
        private readonly Dictionary<String, StringBuilder> _css;

        public const string DefaultCategory = "_default";

        public HtmlSegmentManager()
        {
            _scripts = new Dictionary<string, StringBuilder>();
            _css = new Dictionary<string, StringBuilder>();
        }

        public void AddScript(string scriptContent, string category= null)
        {
            if (!scriptContent.IsNullOrEmpty())
            {
                string c = category.IfNullOrWhiteSpace(DefaultCategory);
                var list = _scripts.GetOrAdd(c, k => new StringBuilder());
                list.AppendLine(scriptContent);
            }
        }

        //public void AddCss(string cssContent, string category = null)
        //{
        //    if (cssContent.IsNullOrWhiteSpace())
        //    {
        //        string c = category.IfNullOrWhiteSpace(DefaultCategory);
        //        var list = _scripts.GetOrAdd(category, k => new StringBuilder());
        //        list.AppendLine(cssContent);
        //    }
        //}

        public String GetAllScripts()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var v in _scripts.Values)
            {
                builder.Append(v);
            }
            var html = builder.ToString();
            if (!html.IsNullOrWhiteSpace())
            {
                return html;
            }
            return String.Empty;
        }

        public string GetScriptCategory(string category = null)
        {
            string c = category.IfNullOrWhiteSpace(DefaultCategory);
            StringBuilder builder = null;
            if (_scripts.TryGetValue(c, out builder))
            {
                if (builder != null)
                {
                    string html = builder.ToString();
                    if (!html.IsNullOrWhiteSpace())
                    {
                        return html;
                    }
                }
            }
            return String.Empty;
        }
    }
}
