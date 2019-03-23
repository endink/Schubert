using Microsoft.Extensions.Logging;
using Schubert.Framework.Environment;
using Schubert.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    public static class LoggerExtensions
    {
        private static readonly Func<LogEntry, Exception, string> LogFormatter = FormatLog;

        private static string FormatLog(LogEntry log, Exception ex)
        {
            StringBuilder builder = new StringBuilder(log.Message.IsNullOrWhiteSpace() ? String.Empty : $"{log.Message.IndentLeft(log.FormatIndent)}{System.Environment.NewLine}");
            builder.AppendLine($"Application:{log.ApplicationName} ver:{log.ApplicationVersion}".IndentLeft(log.FormatIndent));
            if (!log.User.IsNullOrWhiteSpace())
            {
                builder.AppendLine($"User: {log.User}".IndentLeft(log.FormatIndent));
            }
            builder.AppendLine($"Time: {DateTime.UtcNow.ToBeiJingTimeFromUtc().ToString("yyyy-MM-dd HH:mm:ss")}".IndentLeft(log.FormatIndent));
            log.Host = SchubertUtility.GetCurrentIPAddress();
            if (!log.Host.IsNullOrWhiteSpace())
            {
                builder.AppendLine($"Host: {log.Host}".IndentLeft(log.FormatIndent));
            }
            if (SchubertEngine.Current.IsRunning)
            {
                builder.AppendLine($"Environment: {SchubertEngine.Current.Environment}".IndentLeft(log.FormatIndent));
            }
            if (ex != null)
            {
                builder.AppendExcetpion(ex, log.FormatIndent);
            }

            var ext = log.Where(v =>
             !v.Key.CaseInsensitiveEquals(nameof(log.ApplicationVersion)) &&
            !v.Key.CaseInsensitiveEquals(nameof(log.ApplicationName)) &&
            !v.Key.CaseInsensitiveEquals(nameof(log.Host)) &&
            !v.Key.CaseInsensitiveEquals(nameof(log.Message)) &&
            !v.Key.CaseInsensitiveEquals(nameof(log.User))).ToArray();
            if (ext.Any())
            {
                builder.Append($"Other Info :".IndentLeft(log.FormatIndent));
                foreach (var kp in ext)
                {
                    builder.AppendLine($"{kp.Key}: {kp.Value}".IndentLeft(log.FormatIndent));
                }
            }
            string message = builder.ToString();
            return message;
        }

        private static string IndentLeft(this string message, int indent)
        {
            if (message.IsNullOrWhiteSpace())
            {
                return String.Empty;
            }
            var lines = message.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string msg = String.Empty;
            foreach (var l in lines)
            {
                string indentString = String.Empty;
                for (int i = 0; i < indent; i++)
                {
                    indentString = String.Concat(indentString, " ");
                }
                msg = String.Concat(msg, $"{indentString}{l}{System.Environment.NewLine}");
            }
            return msg.Remove(msg.Length - 2, System.Environment.NewLine.Length);
        }

        private static StringBuilder AppendExcetpion(this StringBuilder builder, Exception ex, int indent = 0)
        {
            string prefixEx = $"{ex.GetType().FullName}: ";
            builder.AppendLine($"{prefixEx}{System.Environment.NewLine}{ex.ToString()}".IndentLeft(indent));
            return builder;
        }



        /// <summary>
        /// 记录日志条目。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="log">日志对象。</param>
        /// <param name="ex">引起程序错误的异常。</param>
        /// <param name="extensions">日志扩展信息（匿名对象）。</param>
        internal static void Write(this ILogger logger, LogEntry log, Exception ex = null, object extensions = null)
        {
            var dir = extensions.ToDictionary();
            log.AddRange(dir, false);
            logger.Log(log.LogLevel, log.EventId, log, ex, LogFormatter);
        }

        /// <summary>
        /// 记录错误日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="eventId">事件 Id。</param>
        /// <param name="message">消息。</param>
        /// <param name="ex">引发错误的异常。</param>
        /// <param name="extensions">引发错误的异常。</param>
        public static void WriteError(this ILogger logger, EventId eventId, string message, Exception ex = null, object extensions = null)
        {
            LogEntry log = new LogEntry() { Message = message, EventId = eventId, LogLevel = LogLevel.Error };
            logger.Write(log, ex, extensions);
        }

        /// <summary>
        /// 记录错误日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        public static void WriteError(this ILogger logger, string message)
        {
            logger.WriteError(0, message, null, null);
        }

        /// <summary>
        /// 记录错误日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="ex">引发错误的异常。</param>
        public static void WriteError(this ILogger logger, string message, Exception ex)
        {
            logger.WriteError(0, message, ex, null);
        }

        /// <summary>
        /// 记录调试日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="eventId">事件 Id。</param>
        /// <param name="message">日志消息。</param>
        /// <param name="extensions">扩展信息（匿名对象）。</param>
        public static void WriteDebug(this ILogger logger, EventId eventId, string message, object extensions = null)
        {
            LogEntry log = new LogEntry() { Message = message, EventId = eventId, LogLevel = LogLevel.Debug };
            logger.Write(log, null, extensions);
        }

        /// <summary>
        /// 记录调试日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="extensions">扩展信息（匿名对象）。</param>
        public static void WriteDebug(this ILogger logger, string message, object extensions = null)
        {
            logger.WriteDebug(0, message, extensions);
        }

        /// <summary>
        /// 记录信息日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="eventId">事件 Id。</param>
        /// <param name="extensions">扩展信息（匿名对象）。</param>
        public static void WriteInformation(this ILogger logger, EventId eventId, string message, object extensions = null)
        {
            LogEntry log = new LogEntry() { Message = message, EventId = eventId, LogLevel = LogLevel.Information };
            logger.Write(log, null, extensions);
        }

        /// <summary>
        /// 记录信息日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="extensions">扩展信息（匿名对象）。</param>
        public static void WriteInformation(this ILogger logger, string message, object extensions = null)
        {
            logger.WriteInformation(0, message, extensions);
        }

        /// <summary>
        ///  记录警告日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">错误消息。</param>
        public static void WriteWarning(this ILogger logger, string message)
        {
            logger.WriteWarning(0, message, null, null);
        }

        /// <summary>
        ///  记录警告日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">错误消息。</param>
        /// <param name="exception">异常信息。</param>
        public static void WriteWarning(this ILogger logger, string message, Exception exception)
        {
            logger.WriteWarning(0, message, exception, null);
        }

        /// <summary>
        /// 记录警告日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="eventId">事件 id。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常信息。</param>
        /// <param name="extensions">扩展信息（匿名对象）。</param>
        public static void WriteWarning(this ILogger logger, EventId eventId, string message, Exception exception = null, object extensions = null)
        {
            LogEntry log = new LogEntry() { Message = message, EventId = eventId, LogLevel = LogLevel.Warning };
            logger.Write(log, exception, extensions);
        }

        /// <summary>
        /// 记录灾难日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">错误消息。</param>
        public static void WriteCritical(this ILogger logger, string message)
        {
            logger.WriteCritical(0, message, null, null);
        }

        /// <summary>
        /// 记录灾难日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">错误消息。</param>
        /// <param name="exception">异常对象。</param>
        public static void WriteCritical(this ILogger logger, string message, Exception exception)
        {
            logger.WriteCritical(0, message, exception, null);
        }

        /// <summary>
        /// 记录灾难日志。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="eventId">事件 Id。</param>
        /// <param name="message">错误消息。</param>
        /// <param name="exception">异常信息。</param>
        /// <param name="extensions">扩展信息（匿名对象）。。</param>
        public static void WriteCritical(this ILogger logger, EventId eventId, string message, Exception exception = null, object extensions = null)
        {
            LogEntry log = new LogEntry() { Message = message, EventId = eventId, LogLevel = LogLevel.Critical };
            logger.Write(log, exception, extensions);
        }

        /// <summary>
        /// 记录跟踪日志（最低级别日志）。
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventId">事件 Id。</param>
        /// <param name="message">消息。</param>
        /// <param name="extensions">扩展信息（匿名对象）。</param>
        public static void WriteTrace(this ILogger logger, EventId eventId, string message, object extensions = null)
        {
            LogEntry log = new LogEntry() { Message = message, EventId = eventId, LogLevel = LogLevel.Trace };
            logger.Write(log, null, extensions);
        }

        /// <summary>
        /// 记录跟踪日志（最低级别日志）。
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message">消息。</param>
        /// <param name="extensions">扩展信息（匿名对象）。</param>
        public static void WriteTrace(this ILogger logger, string message, object extensions = null)
        {
            logger.WriteTrace(0, message, extensions);
        }

    }
}
