using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Scheduling
{
    /// <summary>
    /// 表示一个作业调度服务器。
    /// </summary>
    public interface ISchedulingServer
    {
        /// <summary>
        /// 根据作业 Id 移除一个作业。
        /// </summary>
        /// <param name="jobId">要移除的作业 Id 。</param>
        Task<bool> RemoveJobAsync(string jobId);

        /// <summary>
        /// 新增一个作业，如果作业存在则更改作业的执行计划，并返回作业 Id。
        /// </summary>
        /// <param name="jobType">要添加或更新的作业类型。</param>
        /// <param name="cronExpression">表示作业计划的 cron 表达式。</param>
        /// <param name="jobId">要添加的作业 Id（如果为空自动生成 Id）。</param>
        /// <returns>作业 Id。</returns>
        Task<String> AddOrUpdateJobAsync(Type jobType, string cronExpression, string jobId = null);

        /// <summary>
        /// 开始调度作业。
        /// </summary>
        Task ScheduleAsync();
    }
}
