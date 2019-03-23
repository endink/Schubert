using Schubert.Framework.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 表示一个数据库上下文的配置。
    /// </summary>
    internal class DbContextSettings
    {
        public static DbContextSettings CreateDefault(String connectionName)
        {
            return new DbContextSettings(connectionName, typeof(DbContextSettings).GetTypeInfo().Assembly, false, null, null, false, null);
        }

        internal DbContextSettings(
            String connectionName, 
            Assembly migrationAssembly, 
            bool addServiceEntities, 
            IDbProvider provider, 
            IEnumerable<String> tablesForCheck, 
            bool isDefault,
            String connectionString)
            : this(connectionName, migrationAssembly?.FullName, addServiceEntities, provider, tablesForCheck, isDefault, connectionString)
        {
        }

        internal DbContextSettings(
            String connectionName, 
            String migrationAssembly, 
            bool addServiceEntities, 
            IDbProvider provider, 
            IEnumerable<String> tablesForCheck,
            bool isDefaultContext,
            String connectionString)
        {
            this.DbProvider = provider ?? DbOptions.SqliteProvider;
            this.ConnectionName = connectionName;
            this.IncludeServiceEntities = addServiceEntities;
            this.TablesForCheckingDatabaseExists = tablesForCheck ?? Enumerable.Empty<String>();
            this.IsDefault = isDefaultContext;
            this.ConnectionString = connectionString;
        }

        public IEnumerable<String> TablesForCheckingDatabaseExists { get; }

        public IDbProvider DbProvider { get; }


        public bool IncludeServiceEntities { get; }


        public String ConnectionName { get; }

        public String ConnectionString { get; }

        /// <summary>
        /// 获取迁移程序集的名称。
        /// </summary>
        public string MigrationAssembly { get; }

        public bool IsDefault { get; }

    }
}
