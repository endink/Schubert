using Microsoft.EntityFrameworkCore;
using Schubert.Framework.Data.EntityFramework;
using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Schubert.Framework.Data.Providers
{
    public class SqlServerProvider : IDbProvider
    {
        public string IdentifierPrefix => "[";

        public string IdentifierStuffix => "]";
        public string ParameterPrefix => "@";
       

        public PropertyBuilder<string> StringColumnLength(PropertyBuilder<string> pb, int length)
        {
            return pb.HasMaxLength(length);
        }

        public bool ExistTables(DbOptions dbOptions, DbContext dbContext, IEnumerable<string> tableNames)
        {
            if (tableNames.IsNullOrEmpty())
            {
                throw new ArgumentException("tableNames can not be null or empty", nameof(tableNames));
            }
            String conn = dbOptions.GetConnectionString(dbContext.GetType());


            String tableNamesPredicate = tableNames.Select(n => $"[name] = '{n}'").ToArrayString(" or ");
            String sql = $"select  count(*)  from  dbo.sysobjects where {tableNamesPredicate}";

            using (SqlConnection connection = new SqlConnection(conn))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    return tableNames.Count() == Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void OnAddDependencies(IServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkSqlServer();
        }

        public virtual void OnBuildContext(Type dbContextType, DbContextOptionsBuilder builder, DbOptions options)
        {
            string connectionString = options.GetConnectionString(dbContextType);
            string assembly = options.GetMigrationAssembly(dbContextType);

            builder.UseSqlServer(connectionString,
                b =>
                {
                    b.MigrationsAssembly(assembly);
                    b.CommandTimeout((int)options.CommandTimeout.TotalSeconds);
                });
        }

        public virtual void OnCreateModel(ModelBuilder builder, DbOptions options)
        {
            builder.ForSqlServerUseIdentityColumns();
        }

        public bool HasUniqueConstraintViolation(DbUpdateException ex)
        {
            SqlException sex = ex.GetOriginalException<SqlException>();
            return sex?.Errors?.Cast<SqlError>()?.Any(e => e.Number == 2627) ?? false;
        }

        public DbParameter CreateDbParameter(string name, object value)
        {
            return new SqlParameter(name, value);
        }
    }
}
