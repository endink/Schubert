using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Schubert.Framework.Data.Dapper.SQLite
{
    public class SQLiteDatabaseProvider : IDatabaseProvider
    {
        public DbConnection CreateConnection(string connectionString) => new SqliteConnection(connectionString);

        public string IdentifierPrefix => "`";
        public string IdentifierStuffix => "`";
        public string ParameterPrefix => "@";

        public string BuildPaginationTSql(int pageIndex, int pageSize, string selectSegment, string orderBySegment, string whereSegment = null)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(pageIndex)} 参数必须大于等于 0。");
            }
            Guard.ArgumentNullOrWhiteSpaceString(selectSegment, nameof(selectSegment));
            var primarySql = selectSegment;
            if (!whereSegment.IsNullOrWhiteSpace())
            {
                primarySql = string.Concat(primarySql, $" WHERE {whereSegment}");
            }
            if (!orderBySegment.IsNullOrWhiteSpace())
            {
                primarySql = string.Concat(primarySql, orderBySegment);
            }

            if (pageIndex == 0)
            {
                return $" {primarySql} LIMIT {pageSize}";
            }
            var skip = pageIndex * pageSize;
            return $" {primarySql} LIMIT {skip}, {pageSize}";
        }

        public string BuildBatchInsertSql(string tableName, string[] columns, IEnumerable<object[]> values,
            Action<string, object> buildDbParameter) => "";

        public object GetLastedInsertId() => "SELECT LAST_INSERT_ROWID()";

        public bool GetLastedInsertIdSupported => true;
        public bool BuildBatchInsertSqlSupported => false;
    }
}
