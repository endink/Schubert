using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    public static class SchubertApplication
    {
        public static void Run<T>(params string[] args)
            where T : CommandLineStartup, new()
        {
            T startup = new T();
            BuildConfiguration(startup, args);

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSchubertFramework(startup.Configuration, startup.ConfigureServices, startup.ConfigureShellCreationScope);

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            startup.Configure(serviceProvider);

            serviceProvider.StartSchubertEngine();

            ExecuteStartup(startup, serviceProvider);
        }

        private static void ExecuteStartup<T>(T startup, IServiceProvider serviceProvider) where T : CommandLineStartup, new()
        {
            Type t = typeof(T);
            var methodInfo = t.GetTypeInfo().GetMethod("Run", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
            {
                throw new SchubertException($"CommandLineStartup 实现类 {t.Name} 必须具有名为 Run 的实例方法才能启动。");
            }
            var parameters = methodInfo.GetParameters();
            object[] values = new object[0];
            if ((parameters?.Length ?? 0) > 0)
            {
                values = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];
                    if (p.ParameterType.Equals(typeof(Object)))
                    {
                        throw new SchubertException($"CommandLineStartup 实现类 {t.Name} 的 Run 方法参数 {t.Name} 不是 DI 中的依赖项。");
                    }
                    else
                    {
                        try
                        {
                            values[i] = serviceProvider.GetRequiredService(p.ParameterType);
                        }
                        catch (InvalidOperationException)
                        {
                            throw new SchubertException($"CommandLineStartup 实现类 {t.Name} 的 Run 方法参数 {t.Name} 不是 DI 中的依赖项。");
                        }
                    }
                }
            }
            methodInfo.Invoke(startup, values);
        }

        private static void BuildConfiguration<T>(T startup, params string[] args) where T : CommandLineStartup, new()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(SchubertUtility.GetApplicationDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddVariables();
                            
            String env = "production";
            var section = builder.Build().GetSection("Schubert:Env");
            if (section != null)
            {
                env = section.Value.IfNullOrWhiteSpace("production");
            }

            startup.BuildConfiguration(env, builder);
            
            builder.AddEnvironmentVariables();
            try
            {
                builder.AddCommandLine(args);
            }
            catch (NotSupportedException)
            { }
            startup.Configuration = builder.Build();
        }

        public static void Exit()
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
