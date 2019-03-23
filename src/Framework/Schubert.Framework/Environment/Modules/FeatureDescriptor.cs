using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Schubert.Framework.Environment.Modules
{
    /// <summary>
    /// 存储 Feature 信息的对象。
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class FeatureDescriptor
    {
        public FeatureDescriptor()
        {
            this.Dependencies = Enumerable.Empty<String>();
            this.Priority = 1;
        }

        /// <summary>
        /// Feature 名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 获取或设置 feature 的分类（例如可以划分为功能，内容，性能等类型）。
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 加载优先级。
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 包含此 Feature 的模块名称（可以为空，通常为空表示是 Shell Feature）。
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// 该 Feature 依赖的 Feature 的名称集合。
        /// </summary>
        public IEnumerable<string> Dependencies { get; set; }
    }
}