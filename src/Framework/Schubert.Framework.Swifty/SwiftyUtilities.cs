using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.Swifty.ContextModel;
using Swifty;
using Swifty.Nifty.Core;
using Swifty.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Swifty
{
    public static class SwiftyUtilities
    {
        internal static IServiceProvider GetServiceProvider(this IRequestContext context)
        {
            return (context?.GetContextData(ScopeInjectionEventHandler.ContextName) as IServiceScope)?.ServiceProvider;
        }

        public static bool TryGetRemoteServiceAttribute(this Type type, bool exceptionIfAttributeConflict, out RemoteServiceAttribute attribute)
        {
            attribute = null;
            var serverType = type.GetTypeInfo();
            if (!serverType.IsInterface)
            {
                return false;
            }

            var thriftAttribute = serverType.GetCustomAttribute<ThriftServiceAttribute>();
            if (thriftAttribute == null)
            {
                return false;
            }

            var assemblyAttr = serverType.Assembly.GetCustomAttribute<RemoteServiceAttribute>();
            var typeAttr = serverType.GetCustomAttribute<RemoteServiceAttribute>();

            if (assemblyAttr != null && typeAttr != null)
            {
                if (exceptionIfAttributeConflict)
                {
                    throw new SwiftyApplicationException($"{nameof(RemoteServiceAttribute)} 不能同时应用与 Assembly  和 类型，{serverType.FullName} 上的 {nameof(RemoteServiceAttribute)} 违反了该约定。");
                }
                return false;
            }
            if (assemblyAttr == null && typeAttr == null)
            {
                return false;
            }

            attribute = assemblyAttr ?? typeAttr;
            return true;
        }
    }
}
