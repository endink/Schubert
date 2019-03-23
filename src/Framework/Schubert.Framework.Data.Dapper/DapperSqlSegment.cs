using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Text;

namespace Schubert.Framework.Data
{
    internal class DapperSqlSegment
    {
        public DapperSqlSegment()
        {
            this.Parameters = new DynamicParameters();
        }

        public string Sql { get; set; }

        public DynamicParameters Parameters { get; }

        public string GetSqlString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(this.Sql);
            builder.AppendLine("-------------------------------------------");
            builder.AppendLine(this.Parameters.ParameterNames.Select(n => $"@{n} = {this.Parameters.Get<Object>(n)?.ToString().IfNullOrWhiteSpace("null")}").ToArrayString(", "));
            return builder.ToString();
        }
    }
}
