using Microsoft.Extensions.Logging;
using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Services
{
    public interface ILogService : IDependency
    {
        /// <summary>
        /// 按年月分页获取日志记录。
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <param name="level">日志等级</param>
        /// <param name="logScope">日志分区</param>
        /// <returns></returns>
        Task<IEnumerable<ILog>> GetLoggingAsync(int pageIndex, int pageSize, int year, int month, LogLevel? level = null, string logScope = null);

        /// <summary>
        /// 根据年月删除日志表。
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="logScope">日志分区</param>
        /// <returns></returns>
        Task DeleteLoggingAsync(int year, int month, string logScope = null);
    }
}
