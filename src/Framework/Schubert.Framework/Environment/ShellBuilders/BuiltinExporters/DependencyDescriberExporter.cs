using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.Environment.Modules;
using System.Reflection;

namespace Schubert.Framework.Environment.ShellBuilders.BuiltinExporters
{
    public class DependencyDescriberExporter : IShellBlueprintItemExporter
    {
        public string Category
        {
            get { return BuiltinBlueprintItemCategories.DependencyDescriber; }
        }

        public bool CanExport(Type type)
        {
            return typeof(IDependencyDescriber).GetTypeInfo().IsAssignableFrom(type);
        }

        public ShellBlueprintItem Export(Type type, Feature feature)
        {
            return new ShellBlueprintItem { Type = type, Feature = feature };
        }
    }
}
