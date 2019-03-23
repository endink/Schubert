using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.Environment.Modules;
using System.Reflection;

namespace Schubert.Framework.Environment.ShellBuilders.BuiltinExporters
{
    public class DependencyExporter : IShellBlueprintItemExporter
    {
        public string Category
        {
            get { return BuiltinBlueprintItemCategories.Dependency; }
        }

        public bool CanExport(Type type)
        {
            return typeof(IDependency).GetTypeInfo().IsAssignableFrom(type);
        }

        public ShellBlueprintItem Export(Type type, Feature feature)
        {
            return new ShellBlueprintDependencyItem { Type = type, Feature = feature };
        }
    }
}
