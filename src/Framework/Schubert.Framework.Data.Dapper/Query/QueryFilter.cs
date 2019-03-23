using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public abstract class QueryFilter
    {
        internal QueryFilter()
        {

        }

        /// <summary>
        /// 通过 and 语义合并两个查询过滤器。
        /// </summary>
        public static QueryFilter CombineAnd(QueryFilter filter1, QueryFilter filter2)
        {
            return new CombinedQueryFilter(filter1, filter2, BooleanClause.And);
        }

        /// <summary>
        /// 通过 or 语义合并两个查询过滤器。
        /// </summary>
        public static QueryFilter CombineOr(QueryFilter filter1, QueryFilter filter2)
        {
            return new CombinedQueryFilter(filter1, filter2, BooleanClause.Or);
        }
    }
}
