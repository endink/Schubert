using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 名称映射策略
    /// </summary>
    public enum IdentifierMappingStrategy
    {
        /// <summary>
        /// 驼峰命名规则
        /// </summary>
        PascalCase = 0,
        /// <summary>
        /// 下划线分隔
        /// </summary>
        Underline = 1
    }
}
