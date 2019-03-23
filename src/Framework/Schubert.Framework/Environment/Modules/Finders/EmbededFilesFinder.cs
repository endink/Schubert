using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment.Modules.Finders
{
    /// <summary>
    /// 从 Web 项目引用的项目中查找模块（不会检查 Schubert 框架项目）。
    /// </summary>
    public abstract class EmbededFilesFinder : IModuleFinder
    {
        private IEnumerable<IModuleHarvester> _moduleHarvesters;
        private ILogger _logger;
        private IAssemblyReader _reader;

        public EmbededFilesFinder(
            IAssemblyReader assemblyReader,
            ILoggerFactory loggerFactory, 
            IEnumerable<IModuleHarvester> harvesters)
        {
            Guard.ArgumentNotNull(assemblyReader, nameof(assemblyReader));
            _logger = loggerFactory.CreateLogger<EmbededFilesFinder>() ?? (ILogger)NullLogger.Instance;
            _reader = assemblyReader;
            _moduleHarvesters = harvesters ?? Enumerable.Empty<IModuleHarvester>(); 
        }

        /// <summary>
        /// 参见 <see cref="EmbededFilesFinder.FindAvailableModules"/> 方法。
        /// </summary>
        public IEnumerable<ModuleDescriptor> FindAvailableModules()
        {
            ConcurrentDictionary<String, ModuleDescriptor> modules =
                new ConcurrentDictionary<string, ModuleDescriptor>();

            IEnumerable<string> folders = this.GetFolders().Where(f=>Directory.Exists(f));
            Parallel.ForEach(folders, (f) =>
            {
                var files = Directory.EnumerateFiles(f, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(file =>
                {
                    string name = Path.GetFileName(file);
                    return
                    !name.StartsWith("system.", StringComparison.OrdinalIgnoreCase) &&
                    !name.StartsWith("microsoft.", StringComparison.OrdinalIgnoreCase) &&
                    !name.StartsWith("schubert.framework", StringComparison.OrdinalIgnoreCase);
                }).ToArray();
                foreach (var file in files)
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = _reader.ReadFile(file);
                    }
                    catch (BadImageFormatException)
                    {
                        continue;
                    }
                    var resourceNames = assembly.GetManifestResourceNames();
                    foreach (string r in resourceNames)
                    {
                        string resourceFile = GetResourceFileName(assembly, r);
                        foreach (IModuleHarvester harverter in _moduleHarvesters)
                        {
                            if (harverter.CanHarvest(resourceFile))
                            {
                                _logger.WriteDebug($"尝试加载模块嵌入式清单文件 {file} , 资源路径：{r}。");
                                using (Stream stream = assembly.GetManifestResourceStream(r))
                                {
                                    string content = stream.ReadToEnd(System.Text.Encoding.UTF8);
                                    ModuleDescriptor desc = null;
                                    if (harverter.TryHarvestModule(content, out desc))
                                    {
                                        desc.RootDirectory = f;
                                        desc.LibraryPath = file;
                                        desc.ModuleManifest = GetResourceFileName(assembly, r, desc.RootNamespce);
                                        modules.GetOrAdd(desc.Name, desc);
                                    }
                                }
                            }
                        }
                    }
                }
            });
            return modules.Values.ToArray();
        }

        public static string GetResourceFileName(Assembly assembly, string resourceName, string @namespace = null)
        {
            string resourceFile = String.Empty;
            string assemblyName = assembly.GetName().Name;
            @namespace = @namespace.IfNullOrWhiteSpace(assembly.GetName().Name);
            //ASP CORE 和 传统类库嵌入式资源路径不一致。
            if (resourceName.StartsWith(assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                resourceName = resourceName.Substring(assemblyName.Length).TrimStart('.');
            }
            else if (resourceName.StartsWith(@namespace))
            {
                resourceName = resourceName.Substring(@namespace.Length).TrimStart('.');
            }

            string[] segements = resourceName.Split('.');
            for (int i = 0; i < segements.Length; i++)
            {
                string spliter = (i == segements.Length - 1) ? "." : @"\";
                resourceFile = String.Concat(resourceFile, spliter, segements[i]);
            }

            return resourceFile.TrimStart('\\');
        }

        /// <summary>
        /// 获取要加载模块 dll 缩在的目录。
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<String> GetFolders();
    }
}