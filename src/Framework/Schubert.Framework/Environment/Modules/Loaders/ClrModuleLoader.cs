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
    public class ClrModuleLoader : IModuleLoader
    {
        private IAssemblyReader _assemblyReader;
        private ILogger _logger;


        public ClrModuleLoader(ILoggerFactory loggerFactory,
            IAssemblyReader assemblyReader)
        {
            Guard.ArgumentNotNull(assemblyReader, nameof(assemblyReader));
            
            _logger = loggerFactory?.CreateLogger<ClrModuleLoader>() ?? (ILogger)NullLogger.Instance;
            _assemblyReader = assemblyReader;
        }

        public bool TryLoad(ModuleDescriptor descriptor, out ModuleAccess access)
        {
            access = null;
            Assembly assembly = null;
            string libraryPath = descriptor.LibraryPath;
            if (File.Exists(libraryPath))
            {
                try
                {
                    assembly = _assemblyReader.ReadFile(libraryPath);
                }
                catch (BadImageFormatException)
                {
                    this._logger.WriteWarning($"{libraryPath} 不是有效的程序集文件。");
                }
                
            }
            else
            {
                try
                {
                    assembly = _assemblyReader.ReadByName(descriptor.LibraryPath);
                }
                catch (Exception ex)
                {
                    ex.ThrowIfNecessary();
                }
            }
            if (assembly != null)
            {
                access = new ModuleAccess();
                access.Descriptor = descriptor;
                access.Assembly = assembly;
                access.Location = descriptor.LibraryPath;

                access.ExportedTypes = this.LoadTypes(descriptor.ModuleManifest, assembly, descriptor.RootNamespce.IfNullOrWhiteSpace(assembly.GetName().Name));
                return true;
            }
            return false;
        }

        private IEnumerable<Type> LoadTypes(string fileName, Assembly assembly, string rootNamespace)
        {

            var reportTypes = assembly.ExportedTypes
                       .Where(t => t.GetTypeInfo().IsClass &&
                       !t.GetTypeInfo().IsAbstract).ToArray();

            string directory = Path.GetDirectoryName(fileName);
            string ns = String.Concat(rootNamespace, directory.IsNullOrWhiteSpace() ? "" : ".", directory.Replace("/", @"\").Replace(@"\", ".").Trim('.'));
            
            this._logger.WriteTrace($"{this.GetType().Name} 开始导出命名空间 {ns}（manifest: {fileName}）。");

            if (rootNamespace.IsNullOrEmpty()) //根目录下的嵌入式资源文件
            {
                return reportTypes;
            }
            else
            {
                return assembly.ExportedTypes
                       .Where(t => t.GetTypeInfo().IsClass &&
                       !t.GetTypeInfo().IsAbstract && t.Namespace.StartsWith(ns));
            }
        }
    }
}
