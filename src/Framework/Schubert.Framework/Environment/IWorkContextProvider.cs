using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment
{
    public interface IWorkContextProvider
    {
        /// <summary>
        /// 优先级，数字越大，优先级越高。
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 优先级，数字越大，优先级越高。
        /// </summary>
        WorkContext GetWorkContext();
    }
}
