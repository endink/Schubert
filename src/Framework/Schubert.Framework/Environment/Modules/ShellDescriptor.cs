using System;
using System.Collections.Generic;
using System.Linq;

namespace Schubert.Framework.Environment.Modules
{
    /// <summary>
    /// 存储 <see cref="Feature"/> 快照的实例。
    /// </summary>
    public class ShellDescriptor
    {
        public ShellDescriptor()
        {
            DisabledFeatures = Enumerable.Empty<String>();
            Parameters = Enumerable.Empty<ShellParameter>();
        }

        /// <summary>
        /// 序列号，用以区分不同的 Hosting 实例。
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// 运行时要启用的 <see cref="Feature"/> 名称集合。
        /// </summary>
        public IEnumerable<String> DisabledFeatures { get; set; }

        /// <summary>
        /// 运行时要启用的参数 <see cref="ShellParameter"/> 集合（保留字段，暂时无用）。
        /// </summary>
        public IEnumerable<ShellParameter> Parameters { get; set; }
    }

 

    public class ShellParameter
    {
        public string Component { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

}