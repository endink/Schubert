using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Schubert.Framework.Environment
{
    /// <summary>
    /// 基于 <see cref="IServiceScope"/> 的工作上下文实现。
    /// </summary>
    public class ScopedWorkContext : WorkContext
    {
        private IServiceProvider _serviceProvider = null;
        public ScopedWorkContext(IServiceProvider scopedServiceProvider)
        {
            Guard.ArgumentNotNull(scopedServiceProvider, nameof(scopedServiceProvider));
            _serviceProvider = scopedServiceProvider;
        }

        public override object Resolve(Type type)
        {
            return _serviceProvider.GetService(type);
        }

        public override object ResolveRequired(Type type)
        {
            return _serviceProvider.GetRequiredService(type);
        }

        protected override IEnumerable<IWorkContextStateProvider> GetStateProviders()
        {
            return _serviceProvider.GetServices<IWorkContextStateProvider>() ?? Enumerable.Empty<IWorkContextStateProvider>();
        }
    }
}
