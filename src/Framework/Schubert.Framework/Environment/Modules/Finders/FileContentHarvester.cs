using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Schubert.Framework.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment.Modules.Finders
{
    /// <summary>
    /// 派生类实现从配置文件加载模块的功能。
    /// </summary>
    public abstract class FileContentHarvester : IModuleHarvester
    {
        //private string _currentFile;
        private ILogger _logger;

        public FileContentHarvester(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<FileContentHarvester>() ?? (ILogger)NullLogger.Instance;
        }

        /// <summary>
        /// 获取要加载模块配置文件名称。
        /// </summary>
        protected abstract string ManifestFileName { get; }


        public bool CanHarvest(string file)
        {
            if (this.ManifestFileName.IsNullOrWhiteSpace() || file.IsNullOrWhiteSpace())
            {
                return false;
            }
            try
            {
                return Path.GetFileName(file).CaseInsensitiveEquals(this.ManifestFileName.ToLower());
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.WriteError("解析模块时发生错误。", ex);
                return false;
            }
        }

        public bool TryHarvestModule(string fileContent, out ModuleDescriptor descriptor)
        {
            bool success = ReadFromFileContent(fileContent, out descriptor);
            if (success)
            {
                //将默认 Feature 插入从文件读取到的 Feature 列表中。
                descriptor.Features = ((new FeatureDescriptor[] { this.BuildDefaultFeature(descriptor) })
                    .Concat((descriptor.Features ?? Enumerable.Empty<FeatureDescriptor>()))).ToArray();
            };
            return success;
        }

        /// <summary>
        /// 此方法为模块创建一个默认的 <see cref="FeatureDescriptor"/>。
        /// 意味着任何模块都包含至少一个 Feature，该 Feature 表示没有明确通过 <see cref="SchubertFeatureAttribute"/> 属性标记的组件。
        /// </summary>
        private FeatureDescriptor BuildDefaultFeature(ModuleDescriptor moduleDescriptor)
        {
            return new FeatureDescriptor()
            {
                Category = moduleDescriptor.Category,
                Priority = 100, //模块级的 Feature 总是拥有更高的优先级。
                Description = moduleDescriptor.Description,
                ModuleName = moduleDescriptor.Name,
                Name = moduleDescriptor.Name,
                Dependencies = moduleDescriptor.Dependencies
            };
        }

        /// <summary>
        /// 派生类中重写时表示从文件中读取内容来创建 <see cref="ModuleDescriptor"/>。
        /// 注意，其中 Root 和 Path 属性不需要创建，由 <see cref="FileContentHarvester"/> 类自动设置。
        /// </summary>
        /// <param name="fileContent"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected abstract bool ReadFromFileContent(string fileContent, out ModuleDescriptor descriptor);
        

    }
}
