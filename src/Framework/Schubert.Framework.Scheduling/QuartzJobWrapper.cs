using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Schubert.Framework.Environment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading;

namespace Schubert.Framework.Scheduling
{
    internal class QuartzJobWrapper<TJob> : Quartz.IJob
        where TJob : IScheduledJob
    {
        private Type _jobType = null;

        public QuartzJobWrapper()
        {
            _jobType = typeof(TJob);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var fac = SchubertEngine.Current.GetRequiredService<IServiceScopeFactory>();
            var logger = SchubertEngine.Current.GetService<ILoggerFactory>()?.CreateLogger(typeof(TJob));
            var tracing = SchubertEngine.Current.GetRequiredService<IOptions<SchedulingOptions>>().Value?.EnableJobExecutionTracing ?? true;

            using (var s = fac.CreateScope())
            {
                JobContextHolder.Current.Context = new ScopedWorkContext(s.ServiceProvider);
                try
                {
                    IScheduledJob job = CreateScheduledJob(_jobType, logger, s);
                    if (job != null)
                    {
                        await ExecuteJobAsync(context.JobDetail.Key.Name, job, logger, tracing);
                    }
                }
                finally
                {
                    JobContextHolder.Current.Context?.Dispose();
                    JobContextHolder.Current.Context = null;
                }
            }
        }

        private static async Task ExecuteJobAsync(string id, IScheduledJob job, ILogger logger, bool tracingEnabled)
        {
            Stopwatch sw = tracingEnabled ? new Stopwatch() : null;
            
            try
            {
                sw?.Start();
                await job.ExecuteAsync();
                sw?.Stop();
                if (tracingEnabled)
                {
                    logger?.WriteInformation($"Job（Id: {id} , DisplayName: {job.DisplayName}）已执行，耗时：{sw.ElapsedMilliseconds} 毫秒。");
                }
            }
            catch (Exception ex)
            {
                sw?.Stop();
                logger?.WriteError($"Job（Id: {id} , DisplayName: {job.DisplayName}）执行作业时发生错误。", ex);
                ex.ThrowIfNecessary();
            }
            finally
            {
                IDisposable disposable = job as IDisposable;
                disposable?.Dispose();
            }
        }

        private static IScheduledJob CreateScheduledJob(Type jobType, ILogger logger, IServiceScope s)
        {
            try
            {
                IScheduledJob job = (IScheduledJob)ActivatorUtilities.CreateInstance(s.ServiceProvider, jobType);
                return job;
            }
            catch (InvalidOperationException ex)
            {
                logger?.WriteError($"创建 Job 类型 {jobType.FullName} 实例时发生错误。", ex);
            }
            catch (Exception ex)
            {
                logger?.WriteError($"创建 Job 类型 {jobType.FullName} 实例时发生错误。", ex);
                ex.ThrowIfNecessary();
            }

            return null;
        }
    }
}
