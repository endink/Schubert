using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment.Modules.Finders
{
    /// <summary>
    /// 运行目录加载器（同时会搜索 bin 或运行目录，还包括 Moduldes 子目录。）
    /// </summary>
    public class RunningFolderFinder : EmbededFilesFinder
    {
        private IPathProvider _pathProvider;
        public RunningFolderFinder(
            IPathProvider pathProvider,
            IAssemblyReader assemblyReader,
            ILoggerFactory loggerFactory,
            IEnumerable<IModuleHarvester> harvesters)
            :base(assemblyReader, loggerFactory, harvesters)
        {
            Guard.ArgumentNotNull(pathProvider, nameof(pathProvider));
            _pathProvider = pathProvider;
        }

        protected override IEnumerable<string> GetFolders()
        {

            {
                return new string[] { _pathProvider.RootDirectoryPhysicalPath, _pathProvider.MapApplicationPath("~/Modules") };
            }
        }
    }
}
