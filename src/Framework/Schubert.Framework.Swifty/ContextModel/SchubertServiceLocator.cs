using Swifty.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swifty.Nifty.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Schubert.Framework.Swifty.ContextModel
{
    /// <summary>
    /// Schubert 实现的 Swifty 服务定位器。
    /// </summary>
    public class SchubertServiceLocator : IServiceLocator
    {
        private ILogger _logger;
        public SchubertServiceLocator(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<SchubertServiceLocator>() ?? (ILogger)NullLogger.Instance;
        }

        public object GetService(IRequestContext context, Type serviceType)
        {
            var scope = context.GetContextData(ScopeInjectionEventHandler.ContextName) as IServiceScope;
            if (scope == null)
            {
                var exception = new SchubertException($"找不到当前 Swifty 请求的 {nameof(IServiceScope)}.");
                _logger.WriteError(exception.Message);
                throw exception;
            }
            return scope.ServiceProvider.GetRequiredService(serviceType);
        }
    }
}
