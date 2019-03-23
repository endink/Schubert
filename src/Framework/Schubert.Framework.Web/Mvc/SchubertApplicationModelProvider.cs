using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Options;
using Schubert.Framework.Environment.ShellBuilders;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Schubert.Framework.Web.Mvc
{

    public class SchubertApplicationModeProvider : IApplicationModelProvider
    {
        private ShellBlueprint _blueprint = null;
        public const string ModuleRouteKeyName = "module";

        public int Order => 0;

        public SchubertApplicationModeProvider(ShellBlueprint blueprint)
        {
            Guard.ArgumentNotNull(blueprint, nameof(blueprint));
            
            _blueprint = blueprint;
        }

        private ControllerBlueprintItem FindBlueprintItem(TypeInfo typeInfo)
        {
            return _blueprint.Controllers.FirstOrDefault(item => item.Type.Equals(typeInfo.AsType()));
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            foreach (var controller in context.Result.Controllers)
            {
                ControllerBlueprintItem item = this.FindBlueprintItem(controller.ControllerType);
                if (item != null)
                {
                    controller.RouteValues.Add(ModuleRouteKeyName, item.Feature.Descriptor.ModuleName);
                }
            }
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            
        }
    }
}
