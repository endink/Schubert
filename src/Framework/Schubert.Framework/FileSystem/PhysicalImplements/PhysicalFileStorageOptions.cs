using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    /// <summary>
    /// 表示物理文件存储实现的配置选项。
    /// </summary>
    public class PhysicalFileStorageOptions
    {
        private IEnumerable<String> _scopes;

        public IEnumerable<String> IncludeScopes
        {
            get { return (_scopes ?? (_scopes ?? Enumerable.Empty<String>())); }
            set { _scopes = value; }
        }

        /// <summary>
        /// 网络请求和物理存储映射程序。
        /// </summary>
        public IFileRequestMapping FileMapping { get; set; }
    }
}
