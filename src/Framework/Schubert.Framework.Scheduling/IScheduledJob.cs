using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Scheduling
{
    /// <summary>
    /// 表示一个调度作业。
    /// </summary>
    public interface IScheduledJob : ITransientDependency
    {
        /// <summary>
        /// 获取调度作业的显示名称。
        /// </summary>
        string DisplayName { get;}

        /// <summary>
        /// 获取调度作业的描述。
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 执行作业。
        /// </summary>
        Task ExecuteAsync();

    }
}
