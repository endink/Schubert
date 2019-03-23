using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Environment;
using Schubert.Framework.Scheduling;
using System;

namespace Schubert
{
    public static class SchedulingExtensions
    {
        private static Guid _module = Guid.NewGuid();

        /// <summary>
        /// 添加任务调度组件。
        /// </summary>
        /// <param name="builder"><see cref="SchubertServicesBuilder"/> 对象。</param>
        /// <param name="setup">用于配置调度组件的方法。</param>
        /// <returns></returns>
        public static SchubertServicesBuilder AddJobScheduling(this SchubertServicesBuilder builder, Action<SchedulingOptions> setup = null)
        {
            if (builder.AddedModules.Add(_module))
            {
                builder.ServiceCollection.AddSmart(ServiceDescriber.Singleton(typeof(IWorkContextProvider), typeof(JobWorkContextProvider), SmartOptions.Append));
            }
            builder.ServiceCollection.AddSmart(ServiceDescriber.Singleton(typeof(ISchedulingServer), typeof(QuartzSchedulingServer), SmartOptions.TryAdd));
            
            var schedulingConfiguration = builder.Configuration.GetSection("Schubert:Scheduling");
            builder.ServiceCollection.Configure<SchedulingOptions>(schedulingConfiguration);
            if (setup != null)
            {
                builder.ServiceCollection.Configure(setup);
            }
            ShellEvents.EngineStarted -= ShellEvents_OnEngineStarted;
            ShellEvents.EngineStarted += ShellEvents_OnEngineStarted;
            return builder;
        }

        private static void ShellEvents_OnEngineStarted(SchubertOptions options, IServiceProvider serviceProvider)
        {
            var schedulingServer = serviceProvider.GetRequiredService<ISchedulingServer>();
            schedulingServer.ScheduleAsync().GetAwaiter().GetResult();
        }
    }
}
