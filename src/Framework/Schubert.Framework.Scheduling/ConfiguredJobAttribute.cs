using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Scheduling
{
    /// <summary>
    /// 标记一个通过配置计划加载的调度作业。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited =false)]
    public class ConfiguredJobAttribute : Attribute
    {
        public ConfiguredJobAttribute(string jobId)
        {
            Guard.ArgumentNullOrWhiteSpaceString(jobId, nameof(jobId));
            this.JobId = jobId;
        }

        /// <summary>
        /// 获取配置的作业 Id。
        /// </summary>
        public string JobId { get; }
    }
}
