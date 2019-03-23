using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.Environment.Modules;

namespace Schubert.Framework.Environment.ShellBuilders.BuiltinExporters
{
    public class ControllerExporter : IShellBlueprintItemExporter
    {
        public string Category
        {
            get { return BuiltinBlueprintItemCategories.Controller; }
        }

        public bool CanExport(Type type)
        {
            return type.Name.EndsWith("Controller");
        }

        public ShellBlueprintItem Export(Type type, Feature feature)
        {
            var controllerName = type.Name;
            if (controllerName.EndsWith("Controller"))
                controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

            return new ControllerBlueprintItem
            {
                Type = type,
                Feature = feature,
                ControllerName = controllerName,
            };
        }
    }
}
