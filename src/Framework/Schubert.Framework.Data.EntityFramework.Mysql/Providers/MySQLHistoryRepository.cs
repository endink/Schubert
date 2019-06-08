using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Schubert.Framework.Data.Providers
{
    internal class MySQLHistoryRepository : HistoryRepository
    {
        protected MySQLHistoryRepository(HistoryRepositoryDependencies dependencies) : base(dependencies)
        {
        }

        private string EscapeLiteral(string value)
        {
            return this.Dependencies.TypeMappingSource.GetMapping(typeof(String)).GenerateSqlLiteral(value);
        }

        protected override string ExistsSql
        {
            get
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT 1 FROM information_schema.tables ")
                    .AppendLine("WHERE table_name = '")
                    .Append(EscapeLiteral(this.TableName)).Append("' AND table_schema = DATABASE()");
                return sql.ToString();
            }
        }

        protected override bool InterpretExistsResult(object value)
        {
            return value != DBNull.Value;
        }

        public override string GetBeginIfExistsScript(string migrationId)
        {
            Guard.ArgumentNotNullOrEmptyString(migrationId, nameof(migrationId));
            return new StringBuilder().Append("IF EXISTS(SELECT * FROM ")
                .Append(this.SqlGenerationHelper.DelimitIdentifier(this.TableName, this.TableSchema))
                .Append(" WHERE ")
                .Append(this.SqlGenerationHelper.DelimitIdentifier(this.MigrationIdColumnName))
                .Append(" = '").Append(this.EscapeLiteral(migrationId))
                .AppendLine("')")
                .Append("BEGIN").ToString();
        }

        public override string GetBeginIfNotExistsScript(string migrationId)
        {
            Guard.ArgumentNotNullOrEmptyString(migrationId, nameof(migrationId));
            return new StringBuilder()
                .Append("IF NOT EXISTS(SELECT * FROM ")
                .Append(this.SqlGenerationHelper.DelimitIdentifier(this.TableName, this.TableSchema))
                .Append(" WHERE ")
                .Append(this.SqlGenerationHelper.DelimitIdentifier(this.MigrationIdColumnName))
                .Append(" = '").Append(this.EscapeLiteral(migrationId)).AppendLine("')")
                .Append("BEGIN").ToString();
        }

        protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
        {
            history.HasKey(e => e.MigrationId);
            history.Property(h => h.MigrationId).HasMaxLength(64);
            history.Property(h => h.ProductVersion).HasMaxLength(16);
        }

        public override string GetCreateScript()
        {
            String sql = base.GetCreateScript();
            return sql;
        }

        public override string GetCreateIfNotExistsScript()
        {
            return Regex.Replace(this.GetCreateScript(), @"^\W*CREATE\W*TABLE", "CREATE TABLE IF NOT EXISTS");
        }

        public override string GetEndIfScript()
        {
            return "END;" + System.Environment.NewLine;
        }
    }
}
