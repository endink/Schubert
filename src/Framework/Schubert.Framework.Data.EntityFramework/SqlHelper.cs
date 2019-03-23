using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public class SqlHelper
    {
        private DbContext _context;
        private IOptions<DbOptions> _dbOptions;
        private IDbProvider _dbProvider;

        internal SqlHelper(DbContext context, IOptions<DbOptions> dbOptions)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbOptions = dbOptions ?? throw new ArgumentNullException(nameof(dbOptions));
        }

        public IDbProvider Provider => (_dbProvider ?? (_dbProvider = _dbOptions.Value.GetDbProvider(_context.GetType())));

        /// <summary>
        /// 使用特定数据库的标识符包裹主体字段。
        /// </summary>
        /// <param name="ide"></param>
        /// <returns></returns>
        public string EscapeIdentifier(string ide)
        {
            return $"{this.Provider.IdentifierPrefix}{ide}{this.Provider.IdentifierStuffix}";
        }

        /// <summary>
        /// 使用特定数据库的标识符包裹参数字段。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public string EscapeParameter(string parameter)
        {
            return $"{this.Provider.ParameterPrefix}{parameter}";
        }

        public DbParameter CreateParameter(string name, object value)
        {
            return this.Provider.CreateDbParameter(name, value);
        }

        public DbTransactionScope CreateTransactionScope(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            string conn = _dbOptions.Value.GetConnectionStringName(this._context.GetType());
            return new DbTransactionScope(level, conn);
        }

        /// <summary>
        /// 使用格式化字符串来执行 SQL （自动参数化防止 SQL 注入），
        /// 必须使用 <see cref="FormattableString"/> 串才能自动参数化。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<int> ExecuteSqlCommandAsync(FormattableString sql, TimeSpan? timeout)
        {
            return this._context.ExecuteCommandAsync(sql, timeout);
        }

        /// <summary>
        /// 使用原始 SQL 字符串和参数来执行 SQL。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task<int> ExecuteSqlCommandAsync(RawSqlString sql, TimeSpan? timeout = null, params object[] parameters)
        {
            return this._context.ExecuteCommandAsync(sql, parameters, timeout);
        }
    }
}
