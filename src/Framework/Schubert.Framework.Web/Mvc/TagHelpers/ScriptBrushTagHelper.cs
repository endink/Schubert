using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc.TagHelpers
{
    [HtmlTargetElement("script", Attributes = CategoryAttribute)]
    [HtmlTargetElement("script", Attributes = ManagedAttribute)]
    public sealed class ScriptBrushTagHelper : TagHelper
    {
        private const string CategoryAttribute = "sf-brush-category";
        private const string ManagedAttribute = "sf-brush";

        public ScriptBrushTagHelper(WorkContext context)
        {
            this.SegmentManager = context.Resolve<IHtmlSegmentManager>();
        }

        [HtmlAttributeNotBound]
        internal IHtmlSegmentManager SegmentManager { get; set; }

        [HtmlAttributeName(CategoryAttribute)]
        public string Category { get; set; }
        
        [HtmlAttributeName(ManagedAttribute)]
        public BrushMode BrushMode { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = await output.GetChildContentAsync();
            switch (BrushMode)
            {
                case BrushMode.Input:
                    var contentString = content.GetContent();
                    contentString = contentString.Trim(System.Environment.NewLine.ToCharArray());
                    this.SegmentManager.AddScript(contentString, this.Category);
                    output.SuppressOutput();
                    break;
                case BrushMode.OutputCategory:
                    output.Content.SetHtmlContent(content.GetContent());
                    string segment = this.SegmentManager.GetScriptCategory(this.Category);
                    if (!segment.IsNullOrWhiteSpace())
                    {
                        output.Content.AppendHtml(segment);
                    }
                    break;
                case BrushMode.OutputAll:
                default:
                    output.Content.SetHtmlContent(content.GetContent());
                    string all = this.SegmentManager.GetAllScripts();
                    if (!all.IsNullOrWhiteSpace())
                    {
                        output.Content.AppendHtml(all);
                    }
                    break;

            }
        }
    }
}
