using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 表示数据的排序选项。
    /// </summary>
    public class SortOptions
    {
        private List<String> _fieldNames = null;

        /// <summary>
        /// 创建 <see cref="SortOptions"/> 的新实例。
        /// </summary>
        /// <param name="fieldName">排序字段的名称。</param>
        /// <param name="sort">排序方式。</param>
        public SortOptions(string fieldName, SortOrder sort = SortOrder.Ascending)
        {
            Guard.ArgumentNullOrWhiteSpaceString(fieldName, nameof(fieldName));
            this.SortOrder = sort;
            _fieldNames = new List<string>();
            this._fieldNames.Add(fieldName);
        }

        public SortOrder SortOrder { get; set; }

        /// <summary>
        /// 为当前排序选项添加排序字段（字段排序优先规则按添加的顺序）。
        /// </summary>
        /// <param name="fieldName">要添加的字段名。</param>
        /// <returns></returns>
        public SortOptions AddField(string fieldName)
        {
            Guard.ArgumentNullOrWhiteSpaceString(fieldName, nameof(fieldName));
            this._fieldNames.Add(fieldName);
            return this;
        }

        /// <summary>
        /// 获取用于排序的字段。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<String> GetFields()
        {
            return _fieldNames;
        }
    }
}
