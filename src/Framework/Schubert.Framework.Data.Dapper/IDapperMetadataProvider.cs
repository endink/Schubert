using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 实现 Dapper 元数据提供程序。
    /// </summary>
    public interface IDapperMetadataProvider : ISingletonDependency
    {
        /// <summary>
        /// 获取 Dapper 持久化存储元数据。
        /// </summary>
        /// <returns></returns>
        DapperMetadata GetMetadata();
    }
}
