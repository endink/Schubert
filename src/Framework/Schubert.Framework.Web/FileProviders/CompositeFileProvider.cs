using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Schubert.Framework.Web.FileProviders
{
    /// <summary>
    ///  提供组个多个物理目录的文件寻址方式，该类型主要用来提供 View 查找（业务开发人员无需关心该类）。
    /// </summary>
    public class CompositeFileProvider : IFileProvider
    {
        private IDictionary<String, IFileProvider> _providers = null;
        private IFileProvider _fileProvider = null;

        public CompositeFileProvider()
        {
            _providers = new Dictionary<string, IFileProvider>();
            _fileProvider = new NullFileProvider();
        }

        public void SetRootFileProvider(IFileProvider fileProvider)
        {
            Guard.ArgumentNotNull(fileProvider, nameof(fileProvider));
            _fileProvider = fileProvider;
        }

        /// <summary>
        /// 向组合目录中添加映射。
        /// </summary>
        /// <param name="mapping"></param>
        public void AddDirectoryMappings(VirtualDirectoryMapping mapping)
        {
            Guard.ArgumentNotNull(mapping, nameof(mapping));

            string appPath = this.EnsureSubAppDirectory(mapping.VirtualDirectory).ToLower().Trim();
            string physicalPath = mapping.PhysicalDirectory.ToLower().Trim();

            if (_providers.ContainsKey(appPath))
            {
                throw new ArgumentException($"{nameof(CompositeFileProvider)}  中已经存在 {appPath} 的虚拟路径映射。");
            }
            _providers.Add(appPath, new PhysicalFileProvider(physicalPath));
        }

        private string EnsureSubAppDirectory(string directory)
        {
            if (directory.StartsWith("~"))
            {
                directory = directory.Substring(1);
            }

            if (directory.StartsWith("/"))
            {
                directory = directory.Substring(1);
            }

            //if (!directory.EndsWith("/"))
            //{
            //    directory = $"{directory}/";
            //}
            return directory;
        }

        public IDirectoryContents GetDirectoryContents(string subPath)
        {
            Guard.ArgumentNotNull(subPath, nameof(subPath));
            
            string relativePath = null;

            return this.FindProvider(subPath, out relativePath).GetDirectoryContents(relativePath);
        }

        public IFileInfo GetFileInfo(string subPath)
        {
            Guard.ArgumentNotNull(subPath, nameof(subPath));
            subPath =  subPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string relativePath = null;

            return this.FindProvider(subPath, out relativePath).GetFileInfo(relativePath);
        }

        protected virtual IFileProvider FindProvider(string subPath, out string relativePath)
        {
            subPath = this.EnsureSubAppDirectory(subPath).Trim().ToLower();

            var mapping = _providers.FirstOrDefault(p => subPath.StartsWith(p.Key));
            if (mapping.Value != null)
            {
                relativePath = subPath.Replace(mapping.Key, String.Empty);
                relativePath = this.EnsureSubAppDirectory(relativePath);
                return mapping.Value;
            }

            relativePath = subPath;
            return _fileProvider;
        }

        IChangeToken IFileProvider.Watch(string filter)
        {
            string replativePath = null;
            return this.FindProvider(filter, out replativePath).Watch(filter);
        }
    }
}
