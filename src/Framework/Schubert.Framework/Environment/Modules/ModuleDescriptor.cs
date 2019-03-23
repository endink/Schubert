using Schubert.Framework.Environment.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Schubert.Framework.Environment.Modules
{
    [DebuggerDisplay("{Name} ver:{Version}")]
    public class ModuleDescriptor
    {
        public ModuleDescriptor()
        {
            this.SupportVersions = Enumerable.Empty<String>();
            this.Features = Enumerable.Empty<FeatureDescriptor>();
            this.Dependencies = Enumerable.Empty<String>();
        }

        /// <summary>
        /// 模块的根目录（绝对路径）。
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// 模块程序集命名空间。
        /// </summary>
        public string RootNamespce { get; set; }

        /// <summary>
        /// 程序集 dll 路径，使用绝对路径。
        /// </summary>
        public string LibraryPath { get; set; }

        /// <summary>
        /// 模块名称（通常为 ｛Root.自定名称｝名称）。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 模块的分类（例如可以划分为功能，内容，性能等类型）。
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 表示模块的配置清单文件集合（相对于 <see cref="RootDirectory"/>）。
        /// </summary>
        public String ModuleManifest { get; set; }

        /// <summary>
        /// 获取或设置模块描述信息。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 获取或设置模块版本。
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 获取或设置模块支持的应用程序版本。
        /// </summary>
        public IEnumerable<String> SupportVersions { get; set; }

        /// <summary>
        /// 获取或设置模块的依赖项（依赖的模块）。
        /// </summary>
        public IEnumerable<String> Dependencies { get; set; }

        /// <summary>
        /// 获取或设置模块作者。
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 获取或设置一个值，指示模块是否包含用户界面信息。
        /// </summary>
        public bool IncludeUserInterface { get; set; }

        public IEnumerable<FeatureDescriptor> Features { get; set; }

        public string GetManifestFullPath()
        {
            return Path.Combine(this.RootDirectory, this.ModuleManifest);
        }
    }

    public static class ModuleDescriptorExtensions
    {
        public static string GetIdentifier(this ModuleDescriptor descriptor)
        {
            Guard.ArgumentNotNull(descriptor, nameof(descriptor));

            return $@"_m:{descriptor.Name}-{descriptor.Version}";
        }
    }
}