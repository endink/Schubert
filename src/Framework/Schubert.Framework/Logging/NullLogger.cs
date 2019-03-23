using Microsoft.Extensions.Logging;
using System;

namespace Schubert.Framework.Logging
{
    
    public class NullLogger : ILogger
    {
        [Obsolete("NullLogger.Instance 已过时, 使用 Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance 代替.", false)]
        public static readonly ILogger Instance = new NullLogger();
        

        public IDisposable BeginScope<TState>(TState state)
        {
            return LoggerBase.NoopDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}
