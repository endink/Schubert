using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Schubert.Framework.Environment;
using Schubert.Framework.Services;
using Schubert.Framework.Web;
using Schubert.Framework.Web.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Schubert.Framework.Environment.Modules.Finders;
using Schubert.Framework.Environment.ShellBuilders;
using Schubert.Framework.Environment.ShellBuilders.BuiltinExporters;

namespace Schubert.Framework.DependencyInjection
{
    public static class SchubertWebServices
    {
        public static IEnumerable<SmartServiceDescriptor> GetServices(SchubertWebOptions options, bool append = true)
        {
            Guard.ArgumentNotNull(options, nameof(options));

            if (append)
            {
                yield return ServiceDescriber.Transient<IModuleFinder, PackageFinder>(SmartOptions.Append);
                yield return ServiceDescriber.Singleton<IWorkContextProvider, HttpWorkContextProvider>(SmartOptions.Append);
                yield return ServiceDescriber.Transient<IShellBlueprintItemExporter, ControllerExporter>(SmartOptions.Append);
            }

            yield return ServiceDescriber.Singleton<IHttpContextAccessor, HttpContextAccessor>();
            yield return ServiceDescriber.Scoped<HttpWorkContext, HttpWorkContext>();
            yield return ServiceDescriber.Transient<ISchubertEnvironment, AspNetEnvironment>();
            yield return ServiceDescriber.Transient<ICookiesAccessor, CookiesAccessor>();
            
            yield return ServiceDescriber.Scoped<IClientEnvironment, ClientEnvironment>();
            

            if (options.MvcFeatures != MvcFeatures.None)
            {
                yield return ServiceDescriber.Transient<IApplicationModelProvider, SchubertApplicationModeProvider>(SmartOptions.TryAppend);

                if (options.MvcFeatures == MvcFeatures.Full)
                {
                    yield return ServiceDescriber.Scoped<IHtmlSegmentManager, HtmlSegmentManager>();
                }
                
            }
        }
    }
}
