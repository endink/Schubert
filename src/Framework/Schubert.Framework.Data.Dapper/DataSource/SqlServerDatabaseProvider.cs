using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Schubert.Framework.Data.DbConnectionFactories
{
    public sealed class SqlServerDatabaseProvider : IDatabaseProvider
    {
        public bool GetLastedInsertIdSupported
        {
            get { return true; }
        }

        public string IdentifierPrefix => "[";

        public string IdentifierStuffix => "]";
        public string ParameterPrefix => "@";

        public bool BuildBatchInsertSqlSupported => false;

        public string BuildBatchInsertSql(string tableName, string[] columns, IEnumerable<object[]> values, Action<string, object> buildDbParameter)
        {
            return String.Empty;
        }

        public string BuildPaginationTSql(int pageIndex, int pageSize, string selectSegment, string orderBySegment, string whereSegment = null)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(pageIndex)} 参数必须大于等于 0。");
            }

            Guard.ArgumentNullOrWhiteSpaceString(selectSegment, nameof(selectSegment));
            Guard.ArgumentNullOrWhiteSpaceString(orderBySegment, nameof(orderBySegment));

            string spliter = " FROM ";
            int index = selectSegment.IndexOf(spliter, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                var selectSplited = selectSegment.Split(new String[] { spliter }, StringSplitOptions.None);
                if (selectSplited.Length == 2)
                {
                    string clause = $"{selectSplited[0].Trim()}, ROW_NUMBER() OVER({orderBySegment}) AS [ROWNUMBER] FROM {selectSplited[1].Trim()}";
                    if (!whereSegment.IsNullOrWhiteSpace())
                    {
                        clause = String.Concat(clause, $" WHERE {whereSegment}");
                    }
                    clause = String.Concat($" {orderBySegment}");
                    string sql = $"{selectSplited[0].Trim()} FROM ({clause}) WHERE [ROWNUMBER] BETWEEN {(pageIndex & pageSize) + 1} AND {((pageIndex + 1) & pageSize)}";
                    return sql;
                }
            }
            throw new ArgumentException($"{selectSegment} 不是有效的 TSQL 查询语句，请确保 FROM 前后包含空格。");

            /*
             * select * from ( 
　　　　select *, ROW_NUMBER() OVER(Order by a.CreateTime DESC ) AS RowNumber from table_name as a 
　　) as b 
　　where RowNumber BETWEEN 1 and 5 
             */

        }

        public DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public object GetLastedInsertId()
        {
            return "SELECT SCOPE_IDENTITY()";
        }
    }
}
