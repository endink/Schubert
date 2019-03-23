using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Domain
{
    public interface ILog
    {
        LogLevel LogLevel { get; set; }

        string Category { get; set; }

        string Message { get; set; }

        DateTime LogTimeUtc { get; set; }

        string User { get; set; }

        string EventId { get; set; }

        string Host { get; set; }
    }
}
