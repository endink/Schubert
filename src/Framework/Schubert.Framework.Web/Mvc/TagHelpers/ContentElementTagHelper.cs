using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc.TagHelpers
{
    [HtmlTargetElement("textarea", Attributes = ResourceAttributeName)]
    [HtmlTargetElement("p", Attributes = ResourceAttributeName)]
    [HtmlTargetElement("a", Attributes = ResourceAttributeName)]
    [HtmlTargetElement("span", Attributes = ResourceAttributeName)]
    [HtmlTargetElement("label", Attributes = ResourceAttributeName)]
    public class ContentElementTagHelper : TagHelper
    {
        private const string ResourceAttributeName = "sf-resource";

        public ContentElementTagHelper(
            IHtmlGenerator htmlGenerator,
            WorkContext workContext)
        {
            this.Generator = htmlGenerator;
            this.WorkContext = workContext;
        }

        [HtmlAttributeNotBound]
        protected WorkContext WorkContext { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        
        protected internal IHtmlGenerator Generator { get; set; }

        /// <summary>
        /// An expression to be evaluated against the current model.
        /// </summary>
        [HtmlAttributeName("sf-resource")]
        public string Resource { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!Resource.IsNullOrWhiteSpace())
            {
                var localizer = WorkContext.GetLocalizer();
                if (!output.IsContentModified)
                {
                    var content = await output.GetChildContentAsync();
                    string resourceValue = localizer(Resource, String.Empty);
                    if (resourceValue.IsNullOrWhiteSpace())
                    {
                        output.Content.SetContent(content.GetContent());
                    }
                    else
                    {
                        output.Content.SetContent(resourceValue);
                    }
                }
            }
        }
    }
}
