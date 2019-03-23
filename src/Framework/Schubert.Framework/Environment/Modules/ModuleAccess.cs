using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Schubert.Framework.Environment.Modules
{
    /// <summary>
    /// 表示模块访问对象，该对象供 Schubert Framework 内部访问模块代码和资源。
    /// </summary>
    public sealed class ModuleAccess
    {
        /// <summary>
        /// 获取或设置模块的描述信息。
        /// </summary>
        public ModuleDescriptor Descriptor { get; set; }

        /// <summary>
        /// 获取或设置模块的程序集。
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <summary>
        /// 获取或设置模块路径，通常表示包含模块资源的目录。
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 获取或设置模块的导出类型。
        /// </summary>
        public IEnumerable<Type> ExportedTypes { get; set; }
    }
}