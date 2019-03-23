using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Schubert.Framework.Environment.ShellBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Scheduling
{
    public class QuartzSchedulingServer : ISchedulingServer, IDisposable
    {
        private IOptions<SchedulingOptions> _schedulingOptions = null;
        private IScheduler _scheduler = null;
        private bool _disposed = false;
        private Dictionary<String, Type> _jobTypes;
        private bool _started = false;
        private ILoggerFactory _loggerFactory = null;

        public QuartzSchedulingServer(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory scopeFactory,
            ShellContext shellContext,
            IOptions<SchedulingOptions> schedulingOptions)
        {
            Guard.ArgumentNotNull(scopeFactory, nameof(scopeFactory));
            Guard.ArgumentNotNull(shellContext, nameof(shellContext));
            Guard.ArgumentNotNull(schedulingOptions, nameof(schedulingOptions));
            Guard.ArgumentNotNull(scopeFactory, nameof(scopeFactory));

            var schedulerFactory = new Quartz.Impl.StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

            _schedulingOptions = schedulingOptions;

            _loggerFactory = loggerFactory;

            var jobTypes = shellContext.Blueprint.Dependencies
                .Where(d => typeof(IScheduledJob).GetTypeInfo().IsAssignableFrom(d.Type))
                .Select(d => d.Type)
                .Distinct().ToArray();

            _jobTypes = jobTypes.ToDictionary(t =>
            {
                var jobAttribute = t.GetAttribute<ConfiguredJobAttribute>();
                return (jobAttribute != null) ? jobAttribute.JobId : Guid.NewGuid().ToString("N");
            },
                t => t);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        public Task<bool> RemoveJobAsync(string jobId)
        {
            ThrowIfDisposed();

            Guard.ArgumentNullOrWhiteSpaceString(jobId, nameof(jobId));
            return _scheduler.DeleteJob(JobKey.Create(jobId));
        }

        public async Task<String> AddOrUpdateJobAsync(Type jobType, string cronExpression, string jobId)
        {
            ThrowIfDisposed();

            Guard.ArgumentNotNull(jobType, nameof(jobType));
            Guard.TypeIsAssignableFromType(jobType, typeof(IScheduledJob), nameof(jobType));
            
            Guard.ArgumentNullOrWhiteSpaceString(cronExpression, nameof(cronExpression));

            jobId = jobId.IfNullOrWhiteSpace(Guid.NewGuid().ToString("N"));
            var wrapperType = typeof(QuartzJobWrapper<>).MakeGenericType(jobType);

            IJobDetail job = JobBuilder.Create(wrapperType)
                               .WithIdentity(jobId)
                               .Build();

            ITrigger trigger = TriggerBuilder.Create()
                                    .WithCronSchedule(cronExpression)
                                    .WithIdentity($"{jobId}.Trigger")
                                    .Build();

            var triggers = new HashSet<ITrigger>();
            triggers.Add(trigger);
            await _scheduler.ScheduleJob(job, triggers, true);
            return jobId;
        }

        public async Task ScheduleAsync()
        {
            ThrowIfDisposed();
            int count = 0;
            if (!_started)
            {
                var logger = _loggerFactory.CreateLogger<QuartzSchedulingServer>();
                _started = true;
                List<Task> tasks = new List<Task>();
                if (this._schedulingOptions.Value.Jobs.Any())
                {
                    foreach (var jobConfiguration in this._schedulingOptions.Value.Jobs)
                    {
                        Type jobType = null;
                        if (this._jobTypes.TryGetValue(jobConfiguration.Key, out jobType))
                        {
                            var task = this.AddOrUpdateJobAsync(jobType, jobConfiguration.Value, jobConfiguration.Key);
                            tasks.Add(task);
                        }
                        else
                        {
                            logger.WriteWarning($"配置的作业计划找不到对应的作业（Job Name: {jobConfiguration.Key}）。");
                        }
                    }
                    if (tasks.Any())
                    {
                        count = tasks.Count;
                        await Task.WhenAll(tasks.ToArray());
                    }
                }
                await _scheduler.Start();
                logger.WriteDebug($"调度服务器已启动（本次启动加载了 {tasks.Count} 个配置作业）。");
            }
        }

        public async Task ShutdownAsync()
        {
            ThrowIfDisposed();
            if (_started)
            {
                try
                {
                    var logger = _loggerFactory.CreateLogger<QuartzSchedulingServer>();
                    logger.WriteDebug($"调度服务器已经关闭。");
                }
                catch (ObjectDisposedException) {
                }
                await _scheduler.Clear();
                await _scheduler.Shutdown();
                _started = false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                this.ShutdownAsync().GetAwaiter().GetResult();
                _disposed = true;
            }
        }
    }
}
