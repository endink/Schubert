using Microsoft.Build.Construction;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment.Modules.Finders
{
    public class PackageFinder : IModuleFinder
    {
        private IEnumerable<IModuleHarvester> _moduleHarvesters;
        private ILogger _logger;
        private IAssemblyReader _reader;
        private const string ProjectReferenceType = "ProjectReference";
        private const string AssemblyNameElement = "AssemblyName";

        public PackageFinder(
            IAssemblyReader assemblyReader,
            ILoggerFactory loggerFactory,
            IEnumerable<IModuleHarvester> harvesters)
        {
            Guard.ArgumentNotNull(assemblyReader, nameof(assemblyReader));
            _logger = loggerFactory.CreateLogger<PackageFinder>() ?? (ILogger)NullLogger.Instance;
            _reader = assemblyReader;
            _moduleHarvesters = harvesters ?? Enumerable.Empty<IModuleHarvester>();
        }
        
        /// <summary>
        /// 根据指定名称获取父级目录。
        /// </summary>
        private DirectoryInfo GetParentDirectory(string path, string directoryName)
        {
            string root = Path.GetPathRoot(path);
            var parent = Directory.GetParent(path);
            while (parent != null && !parent.FullName.CaseInsensitiveEquals(root) && !parent.Name.CaseInsensitiveEquals(directoryName))
            {
                this.GetParentDirectory(path, directoryName);
            }
            if (!parent.FullName.CaseInsensitiveEquals(directoryName))
            {
                return null;
            }
            return parent;
        }

        /// <summary>
        /// 返回 键为内容目录，和值为程序集路径。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<String, String>> GetPackageFolders()
        {
            string currentFolder = Directory.GetCurrentDirectory();
            if (SchubertUtility.InVisualStudio())
            {
                var files = Directory.EnumerateFiles(currentFolder, "*.csproj", SearchOption.TopDirectoryOnly);
                if (files != null && files.Any())
                {
                    var rootProject = ProjectRootElement.Open(files.First());
                    var items = rootProject.ItemGroups.SelectMany(ig => ig.Items)
                        .Where(it => it.ItemType.CaseInsensitiveEquals(ProjectReferenceType)).ToArray();

                    foreach (var project in items)
                    {
                        String include = project.Include;
                        
                        String fullPath = Path.IsPathRooted(project.Include) ? project.Include : SchubertUtility.CombinePath(currentFolder, include);
                        var proj = ProjectRootElement.Open(fullPath);
                       
                        ProjectPropertyElement[] projectProperties = proj.PropertyGroups.SelectMany(gp => gp.Children.OfType<ProjectPropertyElement>()).ToArray();
                        var assemblyName = projectProperties.Where(p => p.Name.Equals("AssemblyName")).Select(p => p.Value).FirstOrDefault();
                        string path = assemblyName.IfNullOrWhiteSpace(Path.GetFileNameWithoutExtension(proj.FullPath));
                        yield return new KeyValuePair<string, string>(proj.DirectoryPath, path);
                    }
                }
            }
            else
            {
                var packageRoot = Path.GetFullPath("Modules");
                if (Directory.Exists("Modules"))
                {
                    //每次循环都需要查找，先精简查找列表，获得更好性能。
                    var runtimeLibraries = DependencyContext.Default.RuntimeLibraries.Where(
                        l => !l.Name.StartsWith("microsoft", StringComparison.OrdinalIgnoreCase) && !l.Name.StartsWith("system", StringComparison.OrdinalIgnoreCase)).ToArray();

                    foreach (var d in Directory.EnumerateDirectories(packageRoot, "*.*", SearchOption.TopDirectoryOnly))
                    {
                        string name = Path.GetFileName(d.TrimEnd('\\'));
                        var lib = runtimeLibraries.FirstOrDefault(a => a.Name.CaseInsensitiveEquals(name));
                        if (lib != null)
                        {
                            yield return new KeyValuePair<string, string>(d, Path.GetFullPath(lib.RuntimeAssemblyGroups.First().AssetPaths.First()));
                        }
                    }
                }
            }
        }

        public IEnumerable<ModuleDescriptor> FindAvailableModules()
        {
            ConcurrentDictionary<String, ModuleDescriptor> modules =
                new ConcurrentDictionary<string, ModuleDescriptor>();
            
            var folders = this.GetPackageFolders();
            Parallel.ForEach(folders, (f) =>
            {
                if (Directory.Exists(f.Key))
                {
                    var moduleFiles = Directory.EnumerateFiles(f.Key, "*.*", SearchOption.AllDirectories).ToArray();
                    foreach (var file in moduleFiles)
                    {
                        foreach (IModuleHarvester harverter in _moduleHarvesters)
                        {
                            if (harverter.CanHarvest(file))
                            {
                                _logger.WriteDebug($"尝试加载模块物理清单文件 {file} 。");
                                ModuleDescriptor desc = null;
                                if (harverter.TryHarvestModule(File.ReadAllText(file, Encoding.UTF8), out desc))
                                {
                                    desc.RootDirectory = f.Key;
                                    desc.ModuleManifest = SchubertUtility.GetRetrivePath(f.Key, file);
                                    desc.LibraryPath = f.Value;
                                    modules.GetOrAdd(desc.Name, desc);
                                }
                            }
                        }
                    }
                }
            });
            return modules.Values.ToArray();
        }

    }
}
