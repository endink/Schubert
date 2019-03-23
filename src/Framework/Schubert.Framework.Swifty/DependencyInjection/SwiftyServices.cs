using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Environment;
using Schubert.Framework.Swifty.ContextModel;
using Swifty.Nifty.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Swifty.DependencyInjection
{
    public static class SwiftyServices
    {
        public static IEnumerable<SmartServiceDescriptor> GetServices()
        {
            yield return ServiceDescriber.Scoped<SwiftyWorkContext, SwiftyWorkContext>();
            yield return ServiceDescriber.Singleton<IWorkContextProvider, SwiftyWorkContextProvider>(SmartOptions.Append);
        }
    }
}

