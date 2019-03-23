using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 表示多个聚合条件的查询。
    /// </summary>
    public class CombinedQueryFilter : QueryFilter
    {
        public CombinedQueryFilter(QueryFilter filter1, QueryFilter filter2, BooleanClause clause)
        {
            Guard.ArgumentNotNull(filter1, nameof(filter1));
            Guard.ArgumentNotNull(filter2, nameof(filter2));

            this.Filter1 = filter1;
            this.Filter2 = filter2;
            this.Clause = clause;
        }

        public QueryFilter Filter1 { get; }

        public QueryFilter Filter2 { get; }

        /// <summary>
        /// 表示查询过滤器中的字段逻辑组合方式。
        /// </summary>
        public BooleanClause Clause { get; } = BooleanClause.And;
    }
}
