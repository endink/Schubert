using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schubert.Framework.Domain;
using Schubert.Framework.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Logging
{
    public abstract class LoggerBase : ILogger
    {
        private readonly string _name;
        private Func<string, LogLevel, bool> _filter;
        protected string Name
        {
            get
            {
                return this._name;
            }
        }
        
        public LoggerBase(string name, Func<string, LogLevel, bool> filter)
        {
            this._name = name;
            Func<String, LogLevel, bool> defaultFilter = (s, l) => true;
            this._filter = filter ?? defaultFilter;
        }

        public virtual bool IsEnabled(LogLevel logLevel)
        {
            return (_filter == null || _filter(_name, logLevel));
        }
        

        public virtual IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            LogEntry entry = state as LogEntry;
            if (entry != null)
            {
                entry.FormatIndent = $"[{logLevel}]".Length;
            }
            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            var extensions = (state as IEnumerable<KeyValuePair<String, Object>>) ?? Enumerable.Empty<KeyValuePair<String, Object>>();
            message = $"[{logLevel}]{_name}:{System.Environment.NewLine}{message}";
            this.WriteLog(_name, eventId, logLevel, message, extensions);
        }


        protected virtual void WriteLog(string name, EventId eventId, LogLevel level, string message, IEnumerable<KeyValuePair<String, Object>> extensions) { }
        

        public class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();
            
            public void Dispose()
            {
            }
        }

    }
}
