using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    public class HttpWorkContextProvider : IWorkContextProvider
    {
        private IHttpContextAccessor _httpContextAccessor;

        public int Priority => int.MaxValue;

        public HttpWorkContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            Guard.ArgumentNotNull(httpContextAccessor, nameof(httpContextAccessor));
            _httpContextAccessor = httpContextAccessor;
        }

        public WorkContext GetWorkContext()
        {
           return _httpContextAccessor.HttpContext?.RequestServices?.GetService<HttpWorkContext>();
        }
    }
}
