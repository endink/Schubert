using Microsoft.EntityFrameworkCore;
using Schubert.Framework.Domain;
using System;

namespace Schubert.Framework.Data
{
    public class DbContextSettingsBuilder
    {
        private IDbProvider _dbProvider;
        private bool _includeServiceEntities;
        private string _migrationAssembly;
        private String[] _tables;
        private String _connectionString;
        private bool _userPool = true;
        private int _poolSize = System.Environment.ProcessorCount * 2;


        internal DbContextSettingsBuilder(String connectionStringName, String connectionString)
        {
            this.ConnectionName = connectionStringName;
            _connectionString = connectionString;
        }

        internal String ConnectionName { get; }

        internal bool IsDefaultDbContext { get; set; }

        /// <summary>
        /// 是否作为默认的 <see cref="DbContext"/>。
        /// </summary>
        /// <param name="isDefault"></param>
        /// <returns></returns>
        public DbContextSettingsBuilder DefaultContext(bool isDefault)
        {
            this.IsDefaultDbContext = isDefault;
            return this;
        }

        /// <summary>
        /// 使用指定的数据库提供程序。
        /// </summary>
        /// <param name="dbProvider"></param>
        public DbContextSettingsBuilder Provider(IDbProvider dbProvider)
        {
            _dbProvider = dbProvider;
            return this;
        }

        /// <summary>
        /// 指定迁移程序集。
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public DbContextSettingsBuilder MigrationAssembly(String assemblyName)
        {
            _migrationAssembly = assemblyName;
            return this;
        }

        /// <summary>
        /// 设置用于作为判断数据库是否存在的表名（检查表存在性以判断 EF 是否已经创建了这个数据库）。
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        public DbContextSettingsBuilder TablesForCheckingDatabaseExists(params string[] tables)
        {
            _tables = tables;
            return this;
        }

        /// <summary>
        /// 使用 schubert 框架中内置的服务和实体，例如 ：<see cref="Language"/>、<see cref="Permission"/> 等。
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public DbContextSettingsBuilder EnableBuiltinEntitiesAndServices(bool enable)
        {
            _includeServiceEntities = enable;
            return this;
        }

        /// <summary>
        /// 使使用使用 Context 池。
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="poolSize"></param>
        /// <returns></returns>
        public DbContextSettingsBuilder UsePool(bool enable, int poolSize)
        {
            _userPool = enable;
            _poolSize = poolSize;
            return this;
        }

        public DbContextSettingsBuilder UsePool(bool enable)
        {
            _userPool = enable;
            return this;
        }

        internal DbContextSettings Build()
        {
            return new DbContextSettings(this.ConnectionName, 
                _migrationAssembly, 
                _includeServiceEntities, 
                _dbProvider, _tables, 
                this.IsDefaultDbContext, 
                _connectionString,
                _userPool,
                _poolSize);
        }
    }
}
