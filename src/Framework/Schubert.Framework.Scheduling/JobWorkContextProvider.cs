using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Scheduling
{
    public class JobWorkContextProvider : IWorkContextProvider
    {
        public int Priority => 1000;

        public WorkContext GetWorkContext()
        {
            return JobContextHolder.Current.Context;
        }
    }
}
