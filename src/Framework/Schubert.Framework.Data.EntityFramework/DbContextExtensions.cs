using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public static class DbContextExtensions
    {
        public static async Task<int> ExecuteCommandAsync(this DbContext context, RawSqlString sql, object[] parameters, TimeSpan? timeout = null)
        {
            int? previousTimeout = null;
            if (timeout.HasValue)
            {
                previousTimeout = context.Database.GetCommandTimeout();
                context.Database.SetCommandTimeout((int)timeout.Value.TotalMilliseconds);
            }
            try
            {
                int count = await context.Database.ExecuteSqlCommandAsync(sql, parameters);
                return count;
            }
            finally
            {
                if (timeout.HasValue)
                {
                    context.Database.SetCommandTimeout(previousTimeout);
                }
            }
        }

        public static async Task<int> ExecuteCommandAsync(this DbContext context, FormattableString sql, TimeSpan? timeout = null)
        {
            int? previousTimeout = null;
            if (timeout.HasValue)
            {
                previousTimeout = context.Database.GetCommandTimeout();
                context.Database.SetCommandTimeout((int)timeout.Value.TotalMilliseconds);
            }
            try
            {
                int count = await context.Database.ExecuteSqlCommandAsync(sql);
                return count;
            }
            finally
            {
                if (timeout.HasValue)
                {
                    context.Database.SetCommandTimeout(previousTimeout);
                }
            }
        }

        public static IQueryable<T> ExecuteQuery<T>(this DbContext context, FormattableString sql)
            where T : class
        {
            return context.Set<T>().FromSql(sql);
        }
    }
}
