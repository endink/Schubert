using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Schubert.Framework.Data.Providers
{
    public class SqliteProvider : IDbProvider
    {
        public string IdentifierPrefix => "[";

        public string IdentifierStuffix => "]";
        
        public string ParameterPrefix => "@";

        public PropertyBuilder<string> StringColumnLength(PropertyBuilder<string> pb, int length)
        {
            return (length <= 4000) ? pb.HasColumnType($"nvarchar({length})") : pb.HasColumnType("ntext");
        }

        public bool ExistTables(DbOptions dbOptions, DbContext dbContext, IEnumerable<string> tableNames)
        {
            if (tableNames.IsNullOrEmpty())
            {
                throw new ArgumentException("tableNames can not be null or empty", nameof(tableNames));
            }
            String conn = dbOptions.GetConnectionString(dbContext.GetType());

            String tableNamesPredicate = tableNames.Select(n => $"[name] = '{n}'").ToArrayString(" or ");
            String sql = $"select count(*) from sqlite_master where type = 'table' and ({tableNamesPredicate})";

            using (SqliteConnection connection = new SqliteConnection(conn))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    return tableNames.Count() == Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void OnAddDependencies(IServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkSqlite();
        }

        public void OnBuildContext(Type dbContextType, DbContextOptionsBuilder builder, DbOptions options)
        {
            string connectionString = options.GetConnectionString(dbContextType);
            string assembly = options.GetMigrationAssembly(dbContextType);

            builder.UseSqlite(connectionString,
                b =>
                {
                    b.MigrationsAssembly(assembly);
                    b.CommandTimeout((int)options.CommandTimeout.TotalSeconds);
                });
        }

        public void OnCreateModel(ModelBuilder builder, DbOptions options)
        {
        }

        public bool HasUniqueConstraintViolation(DbUpdateException ex)
        {
            return false;
        }

        public DbParameter CreateDbParameter(string name, object value)
        {
            return new SqliteParameter(name, value);
        }
    }
}
