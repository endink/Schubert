using Microsoft.Extensions.Logging;
using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc
{
    internal interface ISchubertController
    {
        WorkContext WorkContext { get; }
        ILoggerFactory LoggerFactory { get; }
    }
}
