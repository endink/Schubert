using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Schubert.Framework.Environment.Modules
{
    /// <summary>
    /// 表示一个应用程序功能。
    /// </summary>
    [DebuggerDisplay("{Descriptor.Name}")]
    public class Feature
    {
        /// <summary>
        /// 关于功能的描述。
        /// </summary>
        public FeatureDescriptor Descriptor { get; set; }

        /// <summary>
        /// 该功能的导出类型。
        /// </summary>
        public IEnumerable<Type> ExportedTypes { get; set; }
    }
}