using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swifty.Nifty.Core;
using Swifty.Services;
using Schubert.Framework.Environment;
using Microsoft.Extensions.DependencyInjection;
using Swifty.Services.Metadata;
using Swifty.MicroServices.Commons;

namespace Schubert.Framework.Swifty.ContextModel
{
    /// <summary>
    /// Scope 注入使用的 event handler。
    /// </summary>
    public class ScopeInjectionEventHandler : ThriftEventHandler
    {
        public const string ContextName = "_service_scope";

        private static readonly string RibbonPingMethod = ThriftMethodMetadata
            .GetQualifiedName(HealthCheckIdentifier.ServiceName, HealthCheckIdentifier.PingMethodName);

        public override object GetContext(string methodName, IRequestContext requestContext)
        {
            if (!methodName.Equals(RibbonPingMethod)) //排除 Swifty Ribbon 实现的健康检查服务。
            {
                // IRequestContext 只考虑 Set，不需要清理，请求结束 Swifty  自动清理资源，自动 dispose
                var serviceScope = SchubertEngine.Current.CreateScope();
                requestContext.SetContextData(ContextName, serviceScope);
                return serviceScope;
            }
            return null;
        }

        public override void Done(object context, string methodName)
        {
            base.Done(context, methodName);
            if (context is IServiceScope scope)
            {
                scope.Dispose();
            }
        }
    }
}
