using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.EntityFrameworkCore.Extensions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Common;

namespace Schubert.Framework.Data.Providers
{
    public class MySqlProvider : IDbProvider
    {
        private static readonly Lazy<MySqlProvider> _default;

        static MySqlProvider()
        {
            _default = new Lazy<MySqlProvider>(() => new MySqlProvider(), true);
        }

        public static MySqlProvider Default { get { return _default.Value; } }

        public string IdentifierPrefix => "`";

        public string IdentifierStuffix => "`";
        public string ParameterPrefix => "@";

        public PropertyBuilder<String> StringColumnLength(PropertyBuilder<String> pb, int length)
        {
            if (length > 21844)
            {
                if (length > 5592404)
                {
                    return pb.HasColumnType("longtext");
                }
                return pb.HasColumnType("mediumtext");
            }
            else
            {
                return pb.HasMaxLength(length);
            }
        }

        public bool ExistTables(DbOptions dbOptions, DbContext dbContext, IEnumerable<String> tableNames)
        {
            if (tableNames.IsNullOrEmpty())
            {
                throw new ArgumentException("tableNames can not be null or empty", nameof(tableNames));
            }
            String conn = dbOptions.GetConnectionString(dbContext.GetType());
            
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                String tableNamesPredicate = tableNames.Select(n => $"TABLE_NAME = '{n}'").ToArrayString(" or ");
                String sql = $"select count(*) from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = '{connection.Database}' and ({tableNamesPredicate}); ";

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
            serviceCollection.AddEntityFrameworkMySQL();
        }

        public virtual void OnBuildContext(Type dbContextType, DbContextOptionsBuilder builder, DbOptions options)
        {
            string connectionString = options.GetConnectionString(dbContextType);
            string assembly = options.GetMigrationAssembly(dbContextType);
            
            builder.UseMySQL(connectionString,
                b => 
                {
                    b.MigrationsAssembly(assembly);
                    b.CommandTimeout((int)options.CommandTimeout.TotalSeconds);
                });
            //builder.ReplaceService<IHistoryRepository, MySQLHistoryRepository>();
        }

        public void OnCreateModel(ModelBuilder builder, DbOptions options)
        {
            
        }

        public bool HasUniqueConstraintViolation(DbUpdateException ex)
        {
            MySqlException sex = ex.GetOriginalException<MySqlException>();
            return (sex?.Number ?? -1) == 1062;
        }

        public DbParameter CreateDbParameter(string name, object value)
        {
            return new MySqlParameter(name, value);
        }
    }
}
