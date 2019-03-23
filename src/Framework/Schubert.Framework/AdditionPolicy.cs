using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    /// <summary>
    /// 表示追加操作策略。
    /// </summary>
    public enum AdditionPolicy
    {
        /// <summary>
        /// 追加项时如果存在则跳过。
        /// </summary>
        SkipExisting,
        /// <summary>
        /// 追加项时如果存在则更新。
        /// </summary>
        ReplaceExisting
    }
}
