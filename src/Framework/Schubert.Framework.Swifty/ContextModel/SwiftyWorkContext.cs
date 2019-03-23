using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Swifty.Nifty.Core;

namespace Schubert.Framework.Swifty.ContextModel
{
    class SwiftyWorkContext : ScopedWorkContext
    {
        public SwiftyWorkContext() 
            : base(SwiftyWorkContext.GetServiceProvider())
        {
        }

        private static IServiceProvider GetServiceProvider()
        {
            var sp = RequestContexts.GetCurrentContext()?.GetServiceProvider();
            if (sp == null)
            {
                throw new SchubertException($"当前环境不存在一个 Swifty 请求上下文，不支持通过上下文获取 {nameof(IServiceProvider)}。");
            }
            return sp;
        }
    }
}
