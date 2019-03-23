using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 表示基本的增删改查 SQL 语句。
    /// </summary>
    public class CrudSqlSegments
    {
        private Lazy<String> _insertSql = null;
        private Lazy<String> _updateSql = null;
        private Lazy<String> _deleteSql = null;
        private Lazy<String> _selectSql = null;

        public CrudSqlSegments(Type entityType, DapperRuntime runtime)
        {
            Guard.ArgumentNotNull(entityType, nameof(entityType));
            var metadata = runtime.GetMetadata(entityType);
            
            this._insertSql = new Lazy<string>(() => GenerateInsertSql(entityType, runtime, metadata));
            this._updateSql = new Lazy<string>(() => GenerateUpdateSql(entityType, runtime, metadata));
            this._deleteSql = new Lazy<string>(() => GenerateDeleteSql(entityType, runtime, metadata));
            this._selectSql = new Lazy<string>(() => GenerateSelectSql(entityType, runtime, metadata));
        }

        #region Static Generate Sql Method

        private static string KeyDelimitSegment(Type entityType, DapperRuntime runtime, DapperMetadata metadata)
        {
            return metadata.Fields.Where(f => f.IsKey).Select(k => $"{runtime.DelimitIdentifier(entityType, k.Name)} =  {runtime.DelimitParameter(entityType, k.Field.Name)}").ToArrayString(" AND ");
        }

        private static string GenerateInsertSql(Type entityType, DapperRuntime runtime, DapperMetadata metadata)
        {
            var fields = metadata.Fields.Where(f => f.Ignore == false && f.AutoGeneration == false).OrderBy(x => x.Field.Name).ToArray();
            string columns = fields.Select(k => runtime.DelimitIdentifier(entityType, k.Name)).ToArrayString(", ");

            string parameters = fields.Select(k => $"{runtime.DelimitParameter(entityType, k.Field.Name)}").ToArrayString(", ");

            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("INSERT INTO ");
            sqlBuilder.Append($"{runtime.DelimitIdentifier(entityType, metadata.TableName)} ");
            sqlBuilder.Append($"({columns}) ");
            sqlBuilder.Append(" VALUES ");
            sqlBuilder.Append($"({parameters})");

            return sqlBuilder.ToString();
        }

        private static string GenerateUpdateSql(Type entityType, DapperRuntime runtime, DapperMetadata metadata)
        {
            var fields = metadata.Fields.Where(f => !f.IsKey && !f.AutoGeneration && !f.Ignore).OrderBy(x => x.Field.Name).ToArray();
            string columnsSetSeg = fields.Select(k => $"{runtime.DelimitIdentifier(entityType, k.Name)} = {runtime.DelimitParameter(entityType,k.Field.Name)}").ToArrayString(", ");
            string whereSeg = KeyDelimitSegment(entityType, runtime, metadata);

            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("UPDATE ");
            sqlBuilder.Append($"{runtime.DelimitIdentifier(entityType, metadata.TableName)} ");
            sqlBuilder.Append($"SET ");
            sqlBuilder.Append(columnsSetSeg);
            if (!string.IsNullOrWhiteSpace(whereSeg))
            {
                sqlBuilder.Append($" WHERE ");
                sqlBuilder.Append(whereSeg);
            }
            return sqlBuilder.ToString();
        }


        private static string GenerateDeleteSql(Type entityType, DapperRuntime runtime, DapperMetadata metadata)
        {
            string whereSeg = KeyDelimitSegment(entityType, runtime, metadata);

            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("DELETE FROM ");
            sqlBuilder.Append($"{runtime.DelimitIdentifier(entityType, metadata.TableName)} ");
            if (!string.IsNullOrWhiteSpace(whereSeg))
            {
                sqlBuilder.Append($" WHERE ");
                sqlBuilder.Append(whereSeg);
            }
            return sqlBuilder.ToString();
        }

        private static string GenerateSelectSql(Type entityType, DapperRuntime runtime, DapperMetadata metadata)
        {
            string columnsSetSeg =
            metadata.Fields.Where(f => !f.Ignore).OrderBy(x => x.Field.Name).Select(k => runtime.DelimitIdentifier(entityType, k.Name)).ToArrayString(", ");

            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT ");
            sqlBuilder.Append(columnsSetSeg);
            sqlBuilder.Append($" FROM ");
            sqlBuilder.Append($"{runtime.DelimitIdentifier(entityType, metadata.TableName)}");

            return sqlBuilder.ToString();
        }

        #endregion

        public string InsertSql => this._insertSql.Value;

        public string UpdateSql => this._updateSql.Value;

        public string DeleteSql => this._deleteSql.Value;

        public string SelectSql => this._selectSql.Value;
    }
}
