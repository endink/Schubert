using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Schubert.Framework.Domain;

namespace Schubert.Framework.Services
{
    public class NullLogService : ILogService
    {
        public Task DeleteLoggingAsync(int year, int month, string LogTablePrefix = null)
        {
            return Task.FromResult(0);
        }

        public async Task<IEnumerable<ILog>> GetLoggingAsync(int pageIndex, int pageSize, int year, int month, LogLevel? level = null, string LogTablePrefix = null)
        {
            return await Task.FromResult<IEnumerable<ILog>>(Enumerable.Empty<ILog>());
        }
    }
}
