using Microsoft.EntityFrameworkCore;
using Schubert.Framework.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// Data 功能选项（默认配置节 Schubert : Data）
    /// </summary>
    public sealed class DbOptions : DatabaseOptions
    {
        private static readonly Lazy<SqlServerProvider> _sqlServerProvider = new Lazy<SqlServerProvider>(true);
        private static readonly Lazy<SqliteProvider> _sqliteProvider = new Lazy<SqliteProvider>(true);


        private Dictionary<Type, DbContextSettings> _dbContextSettings;
        private Type _defaultContext;


        public static SqlServerProvider SqlServerProvider => _sqlServerProvider.Value;

        public static SqliteProvider SqliteProvider => _sqliteProvider.Value;

        /// <summary>
        /// 获取或设置一个值，指示是否允许在数据库不存在时创建数据库（默认为 true）。
        /// </summary>
        public bool CreateDatabaseIfNotExisting { get; set; } = true;
        
        /// <summary>
        /// 获取或设置命令超时时间（默认为 1 分钟）。
        /// </summary>
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(1);
        
        internal Dictionary<Type, DbContextSettings> DbContextSettings
        {
            get { return _dbContextSettings ?? (_dbContextSettings = new Dictionary<Type, DbContextSettings>(0)); }
            set { _dbContextSettings = value; }
        }
        
        /// <summary>
        /// 获取默认的数据上下文（全局唯一依赖项 <see cref="DbContext"/>）。
        /// </summary>
        public Type DefaultDbContext
        {
            get
            {
                if (_defaultContext == null)
                {
                    var kv = this.DbContextSettings.Where(s => s.Value.IsDefault).FirstOrDefault();
                    if (kv.Key != null)
                    {
                        _defaultContext = kv.Key;
                    }
                }
                return _defaultContext;
            }
        }
    }

    public static class DbOptionsExtensions
    {
        public static bool IsDefaultContext(this DbOptions options, Type dbContextType)
        {
            var settings = options.DbContextSettings.GetOrDefault(dbContextType, () => Data.DbContextSettings.CreateDefault(options.DefaultConnectionName));
            return settings.IsDefault;
        }

        public static IDbProvider GetDbProvider(this DbOptions options, Type dbContextType)
        {
            var settings = options.DbContextSettings.GetOrDefault(dbContextType, () => Data.DbContextSettings.CreateDefault(options.DefaultConnectionName));
            return settings.DbProvider;
        }

        public static bool IncludeBuiltinEntities(this DbOptions options, Type dbContextType)
        {
            var settings = options.DbContextSettings.GetOrDefault(dbContextType, () => Data.DbContextSettings.CreateDefault(options.DefaultConnectionName));
            return settings.IncludeServiceEntities;
        }

        public static String GetConnectionStringName(this DbOptions options, Type dbContextType)
        {
            var settings = options.DbContextSettings.GetOrDefault(dbContextType, () => Data.DbContextSettings.CreateDefault(options.DefaultConnectionName));
            return settings.ConnectionName;
        }

        public static String GetConnectionString(this DbOptions options, Type dbContextType)
        {
            var settings = options.DbContextSettings.GetOrDefault(dbContextType, () => Data.DbContextSettings.CreateDefault(options.DefaultConnectionName));
            return settings.ConnectionString.IfNullOrWhiteSpace(options.GetConnectionString(settings.ConnectionName));
        }

        public static String GetMigrationAssembly(this DbOptions options, Type dbContextType)
        {
            var settings = options.DbContextSettings.GetOrDefault(dbContextType, () => Data.DbContextSettings.CreateDefault(options.DefaultConnectionName));
            return settings.MigrationAssembly.IfNullOrWhiteSpace(dbContextType.GetTypeInfo().Assembly.FullName);
        }

        public static IEnumerable<String> GetTablesForChecking(this DbOptions options, Type dbContextType)
        {
            var settings = options.DbContextSettings.GetOrDefault(dbContextType, () => Data.DbContextSettings.CreateDefault(options.DefaultConnectionName));
            return settings.TablesForCheckingDatabaseExists;
        }
    }
}
