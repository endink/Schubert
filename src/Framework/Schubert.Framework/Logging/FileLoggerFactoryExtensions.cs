using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Schubert.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Schubert
{
    public static class FileLoggerFactoryExtensions
    {
        public static ILoggerFactory AddFile(this ILoggerFactory factory, string folder = "logs", Func<string, LogLevel, bool> filter = null, int backlogSizeKB = 10 * 1024)
        {
            factory.AddProvider(new FileLoggerProvider(folder, filter, backlogSizeKB));
            return factory;
        }

        public static ILoggerFactory AddFile(this ILoggerFactory factory, LogLevel minLevel, string folder = "logs", int backlogSizeKB = 10 * 1024)
        {
            return factory.AddFile(folder, (s, l) => l >= minLevel, backlogSizeKB);
        }

        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerOptions> configure = null)
        {
            //builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());

            builder.Services.Add(
                ServiceDescriptor.Singleton<IConfigureOptions<FileLoggerOptions>, FileLoggerOptionsSetup>()
            );
            //builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<FileLoggerOptions>, LoggerProviderOptionsChangeTokenSource<ConsoleLoggerOptions, ConsoleLoggerProvider>>());
            if (configure != null)
            {
                builder.Services.Configure(configure);
            }
            return builder;
        }
    }
}
