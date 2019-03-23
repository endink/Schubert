using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment.Modules.Finders
{
    public class EntryPointFinder : IModuleFinder
    {
        public IEnumerable<ModuleDescriptor> FindAvailableModules()
        {
            try
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    string fullPath = new Uri(assembly.CodeBase).LocalPath;
                    return new ModuleDescriptor[]
                    {
                        new ModuleDescriptor
                        {
                            Description = "application entry point.", IncludeUserInterface = false,
                            Name = "__entry",
                            LibraryPath = fullPath,
                            RootDirectory = Path.GetDirectoryName(fullPath),
                            Version = assembly.GetName().Version.ToString(),
                            ModuleManifest = Path.GetFileName(fullPath),
                            Features =new FeatureDescriptor[]
                            {
                                new FeatureDescriptor
                                {
                                    Category = "Builtin",
                                    Description = "application entry point.",
                                    ModuleName = "__entry",
                                    Name = "__entry",
                                    Priority = 100
                                }
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                ex.ThrowIfNecessary();
            }

            return Enumerable.Empty<ModuleDescriptor>();
        }
    }
}
