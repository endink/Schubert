using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Schubert.Framework.Environment;
using Microsoft.AspNetCore.Http;

namespace Schubert.Framework.Web
{
    class HttpWorkContext : WorkContext
    {
        private IHttpContextAccessor _httpContextAccessor;
        public HttpWorkContext(IHttpContextAccessor httpContextAccessor)
        {
            Guard.ArgumentNotNull(httpContextAccessor, nameof(httpContextAccessor));

            _httpContextAccessor = httpContextAccessor;
        }

        public override object Resolve(Type type)
        {
            var provider = _httpContextAccessor?.HttpContext?.RequestServices;
            if (provider == null)
            {
                return null;
            }
            return provider.GetService(type);
        }

        public override object ResolveRequired(Type type)
        {
            var provider = _httpContextAccessor?.HttpContext?.RequestServices;
            if (provider == null)
            {
                throw new SchubertException($"当前上下文不可用。");
            }
            return provider.GetRequiredService(type);
        }

        protected override IEnumerable<IWorkContextStateProvider> GetStateProviders()
        {
            return _httpContextAccessor?.HttpContext?.RequestServices?.GetServices<IWorkContextStateProvider>() ?? Enumerable.Empty<IWorkContextStateProvider>();
        }
    }
}
