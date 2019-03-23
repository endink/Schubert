using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Schubert.Framework.Environment;
using Schubert.Framework.Environment.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Schubert.Framework.Web.FileProviders
{
    /// <summary>
    ///  模块文件提供程序（支持路径格式 {moduleName}://...）。
    /// </summary>
    public class ModuleFileProvider : IFileProvider, IDisposable
    {
        private IDictionary<String, PhysicalFileProvider> _providers = null;
        private IFileProvider _fileProvider = null;
        private readonly Regex _pathRegex = null;

        public ModuleFileProvider(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider ?? new NullFileProvider();

            _pathRegex = new Regex(@"/?modules/([^/]+)(/.+)", RegexOptions.IgnoreCase);
        }

        private void EnsureInit()
        {
            if (_providers == null)
            {
                lock (this)
                {
                    if (_providers == null)
                    {
                        IModuleManager manager = (IModuleManager)SchubertEngine.Current.GetService(typeof(IModuleManager));
                        _providers = manager.GetAvailableModules().ToDictionary(
                            m => m.Name,
                            m=>(new PhysicalFileProvider(Path.Combine(m.RootDirectory, Path.GetDirectoryName(m.ModuleManifest)))),
                            StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
        }

        private IFileProvider ParseFileProvider(string subPath, out string path)
        {
            this.EnsureInit();
            path = subPath;

            var match = _pathRegex.Match(subPath.Replace('\\', '/'));
            if (match.Success)
            {
                PhysicalFileProvider provider = null;
                string moduleName = match.Groups[1].Value;
                if (this._providers.TryGetValue(moduleName, out provider))
                {
                    path = match.Groups[2].Value;
                    return provider;
                }
                else
                {
                    throw new SchubertException($"找不到模块 {moduleName}。");
                }
            }
            else
            {
                return _fileProvider;
            }
        }

        public IDirectoryContents GetDirectoryContents(string subPath)
        {
            Guard.ArgumentNullOrWhiteSpaceString(subPath, nameof(subPath));

            string path = null;
            var provider = this.ParseFileProvider(subPath, out path);
            return provider.GetDirectoryContents(path);
        }

        public IFileInfo GetFileInfo(string subPath)
        {
            Guard.ArgumentNullOrWhiteSpaceString(subPath, nameof(subPath));

            string path = null;
            var provider = this.ParseFileProvider(subPath, out path);
            return provider.GetFileInfo(path);
        }

        public IChangeToken Watch(string filter)
        {
            string path = null;
            var provider = this.ParseFileProvider(filter, out path);
            return provider.Watch(path);
        }

        public void Dispose()
        {
            var providers = this._providers.Values.ToArray();
            this._providers.Clear();
            foreach (var p in providers)
            {
                p.Dispose();
            }
        }
    }
}
