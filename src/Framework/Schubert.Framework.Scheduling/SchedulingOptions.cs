using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Scheduling
{
    /// <summary>
    /// 表示任务调度的选项。
    /// </summary>
    public class SchedulingOptions
    {
        private Dictionary<String, String> _jobs = null;

        public SchedulingOptions()
        {
            this.EnableJobExecutionTracing = true;
        }

        /// <summary>
        /// 获取或设置一个值，指示是否对作业执行进行追踪（日志记录），默认为 true
        /// </summary>
        public bool EnableJobExecutionTracing { get; set; }

        /// <summary>
        /// 获取或设置任务和
        /// </summary>
        public Dictionary<String, String> Jobs
        {
            get { return _jobs ?? (_jobs = new Dictionary<string, string>()); }
            set { _jobs = value; }
        }
    }
}
