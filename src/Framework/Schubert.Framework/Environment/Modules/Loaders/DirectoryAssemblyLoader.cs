using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment.Modules.Loaders
{
    public class DirectoryAssemblyLoader : IModuleLoader
    {
        private IAssemblyReader _assemblyReader;
        private ILogger _logger;
        private ConcurrentDictionary<String, IEnumerable<String>> _assemblyNamespaces = null;


        public DirectoryAssemblyLoader(ILoggerFactory loggerFactory,
            IAssemblyReader assemblyReader)
        {
            Guard.ArgumentNotNull(assemblyReader, nameof(assemblyReader));

            _assemblyNamespaces = new ConcurrentDictionary<String, IEnumerable<String>>();
            _logger = loggerFactory?.CreateLogger<DirectoryAssemblyLoader>() ?? (ILogger)NullLogger.Instance;
            _assemblyReader = assemblyReader;
        }

        public bool TryLoad(ModuleDescriptor descriptor, out ModuleAccess access)
        {
            //对于 Full Framework :
            //ModuleDescriptor.Path ：（嵌入的模块配置文件）资源名称。
            //ModuleDescriptor.Root ：程序集文件名。

            access = null;
            if (File.Exists(descriptor.LibraryPath))
            {
                string resourceName = descriptor.ModuleManifest;
                Assembly assembly = null;
                try
                {
                    assembly = _assemblyReader.ReadFile(descriptor.LibraryPath);
                    access = new ModuleAccess();
                    access.Descriptor = descriptor;
                    access.Assembly = assembly;
                    access.Location = descriptor.LibraryPath;
                    access.ExportedTypes = this.LoadTypes(descriptor.Name, assembly);
                    return true;
                }
                catch (BadImageFormatException)
                { }
            }
            return false;
        }

        private IEnumerable<Type> LoadTypes(string fileName, Assembly assembly)
        {
            var reportTypes = assembly.ExportedTypes
                       .Where(t => t.GetTypeInfo().IsClass &&
                       !t.GetTypeInfo().IsAbstract).ToArray();

            var namespaces = _assemblyNamespaces.GetOrAdd(assembly.FullName.ToLower(), k =>
            {
                var names = reportTypes
                       .OrderByDescending(n => n.Namespace.Length)
                       .Select(t => t.Namespace).Distinct().ToArray();
                return new HashSet<String>(names);
            });
            string rootNamespace = namespaces.FirstOrDefault(n => fileName.StartsWith(n));
            if (rootNamespace.IsNullOrEmpty()) //根目录下的嵌入式资源文件
            {
                return reportTypes;
            }
            else
            {
                return assembly.ExportedTypes
                       .Where(t => t.GetTypeInfo().IsClass &&
                       !t.GetTypeInfo().IsAbstract && t.Namespace.StartsWith(rootNamespace));
            }
        }
    }
}
