using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swifty.Services;
using Swifty.Nifty.Core;
using Schubert.Framework.Swifty;
using Microsoft.Extensions.DependencyInjection;

namespace Schubert.Framework.Swifty.ContextModel
{
    public class SwiftyWorkContextProvider : IWorkContextProvider
    {
        public int Priority => 100;

        public WorkContext GetWorkContext()
        {
            var requestContext = RequestContexts.GetCurrentContext();
            return requestContext?.GetServiceProvider()?.GetService<SwiftyWorkContext>();
        }
    }
}
