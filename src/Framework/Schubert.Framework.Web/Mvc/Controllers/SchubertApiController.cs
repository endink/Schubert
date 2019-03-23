using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Schubert.Framework.Environment;
using Schubert.Framework.Localization;
using System;

namespace Schubert.Framework.Web.Mvc
{
    [WebApi]
    public abstract class SchubertApiController : ControllerBase, ISchubertController
    {
        private ILogger _defaultLogger = null;
        private ILogger _businessLogger = null;
        private WorkContext _context = null;
        private ILoggerFactory _loggerFactory = null;

        private static string Null(string key, string defaultString, object[] args)
        {
            return String.Empty;
        }

        protected virtual string BusinessCategoryName { get { return null; } }

        /// <summary>
        /// 获取提本地化的方法。
        /// </summary>
        public Localizer T
        {
            get { return this.WorkContext?.GetLocalizer() ?? Null; }
        }

        /// <summary>
        /// 获取当前的工作上下文。
        /// </summary>
        protected WorkContext WorkContext
        {
            get { return _context ?? (_context = this.HttpContext.RequestServices.GetService<WorkContext>()); }
        }
        /// <summary>
        /// 获取 <see cref="ILoggerFactory"/> 对象。
        /// </summary>
        protected ILoggerFactory LoggerFactory
        {
            get { return _loggerFactory ?? (_loggerFactory = this.HttpContext.RequestServices.GetService<ILoggerFactory>()); }
        }

        protected ILogger ApplicationLogger
        {
            get
            {
                if (_defaultLogger == null)
                {
                    _defaultLogger = this.LoggerFactory?.CreateLogger(this.GetType().FullName) ?? (ILogger)NullLogger.Instance;
                }
                return _defaultLogger;
            }
        }

        protected ILogger BusinessLogger
        {
            get
            {
                if (_businessLogger == null)
                {
                    if (!this.BusinessCategoryName.IsNullOrWhiteSpace())
                    {
                        _businessLogger = this.LoggerFactory?.CreateLogger(this.BusinessCategoryName) ?? (ILogger)NullLogger.Instance;
                    }
                }
                return _defaultLogger;
            }
        }

        WorkContext ISchubertController.WorkContext
        {
            get { return this.WorkContext; }
        }

        ILoggerFactory ISchubertController.LoggerFactory
        {
            get { return this.LoggerFactory; }
        }
    }
}
