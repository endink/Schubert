using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public sealed class MySqlDatabaseProvider : IDatabaseProvider
    {
        public bool GetLastedInsertIdSupported
        {
            get { return true; }
        }

        public string IdentifierPrefix => "`";

        public string IdentifierStuffix => "`";
        public string ParameterPrefix => "@";

        public bool BuildBatchInsertSqlSupported => true;

        public string BuildBatchInsertSql(string tableName, string[] columns, IEnumerable<object[]> values, Action<String, Object> buildDbParameter)
        {
            if (values.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(values), "BuildBatchInsertSql 参数 values 不能为空或空数组。");
            }
            if (columns.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(columns), "BuildBatchInsertSql 参数 columns 不能为空或空数组。");
            }
            if (values.Any(v => v.Length != columns.Length))
            {
                throw new ArgumentException("BuildBatchInsertSql 参数 columns 的长度和 values 中数组的长度不一致，即参数值的数量和列的数量不一致。");
            }
            string columnNames = columns.Select(c => this.DelimitIdentifier(c)).ToArrayString(", ");

            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("INSERT INTO ");
            sqlBuilder.Append($"{this.DelimitIdentifier(tableName)} ");
            sqlBuilder.Append($"({columnNames})");
            sqlBuilder.Append(" VALUES ");

            int valueIndex = 0;
            var valueArray = values.ToArray();
            foreach (var v in valueArray)
            {
                var parameters = columns.Select(c => $"{this.DelimitParameter(String.Concat(c, valueIndex))}").ToArray();
                if(valueIndex > 0)
                {
                    sqlBuilder.Append(", ");
                }
                sqlBuilder.Append($"({parameters.ToArrayString(", ")})");
                for (int i =0; i < parameters.Length; i++)
                {
                    buildDbParameter?.Invoke(parameters[i], valueArray[valueIndex][i]);
                }
                valueIndex++;
            }

            return sqlBuilder.ToString();
        }

        public string BuildPaginationTSql(int pageIndex, int pageSize, string selectSegment, string orderBySegment, string whereSegment = null)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(pageIndex)} 参数必须大于等于 0。");
            }
            Guard.ArgumentNullOrWhiteSpaceString(selectSegment, nameof(selectSegment));
            string primarySql = selectSegment;
            if (!whereSegment.IsNullOrWhiteSpace())
            {
                primarySql = String.Concat(primarySql, $" WHERE {whereSegment}");
            }
            if (!orderBySegment.IsNullOrWhiteSpace())
            {
                primarySql = String.Concat(primarySql, orderBySegment);
            }

            if (pageIndex == 0)
            {
                return $" {primarySql} LIMIT {pageSize}";
            }
            else
            {
                int skip = pageIndex * pageSize;
                return $" {primarySql} LIMIT {skip}, {pageSize}";
            }
        }

        public DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public object GetLastedInsertId()
        {
            return "SELECT LAST_INSERT_ID()";
        }
    }
}
