using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Threading
{
    public interface ITimerTask
    {
        void Run(ITimeout timeout);
    }
}
