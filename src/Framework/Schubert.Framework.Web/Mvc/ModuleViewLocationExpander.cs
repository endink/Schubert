using Microsoft.AspNetCore.Mvc.Razor;
using Schubert.Framework.Environment;
using Schubert.Framework.Environment.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc
{
    public class ModuleViewLocationExpander : IViewLocationExpander
    {

        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            if (context.Values.ContainsKey(SchubertApplicationModeProvider.ModuleRouteKeyName))
            {
                var moduleName = RazorViewEngine.GetNormalizedRouteValue(context.ActionContext, SchubertApplicationModeProvider.ModuleRouteKeyName);

                viewLocations = viewLocations.Select(lo => $"/Modules/{moduleName}/{lo.TrimStart('/')}");
                viewLocations = viewLocations.Union(new string[] { "/Views/Shared/{0}.cshtml" }).ToArray();
            }
            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var moduleMeatadata = context.ActionContext.ActionDescriptor.RouteValues.FirstOrDefault(
                s => s.Key.CaseInsensitiveEquals(SchubertApplicationModeProvider.ModuleRouteKeyName) && !(s.Value?.ToString()).IsNullOrWhiteSpace());

            if (!moduleMeatadata.Key.IsNullOrWhiteSpace())
            {
                context.Values[SchubertApplicationModeProvider.ModuleRouteKeyName] = moduleMeatadata.Value.ToString();
            }
        }

    }
}
