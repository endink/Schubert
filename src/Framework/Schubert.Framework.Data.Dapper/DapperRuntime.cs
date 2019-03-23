using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Schubert.Framework.Data.Conventions;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 表示 Dapper 运行时环境（包含实体持久化配置和数据库连接等信息）。
    /// </summary>
    public sealed class DapperRuntime
    {
        private ConcurrentDictionary<Type, DapperMetadata> _metadataSet = null;
        private IOptions<DapperDatabaseOptions> _options = null;
        private ConcurrentDictionary<String, IDatabaseProvider> _databaseProviders = null;
        private Lazy<IDatabaseProvider> _defaultDatabaseProvider = null;
        private ConcurrentDictionary<Type, DapperDataSource> _mappedDataSources = null;
        private ConcurrentDictionary<Type, CrudSqlSegments> _crudSegments = null;
        private DapperSqlGenerator _sqlGenerator = null;

        public DapperRuntime(
            IOptions<DapperDatabaseOptions> options,
            IEnumerable<IDapperMetadataProvider> metadataProviders)
        {
            Guard.ArgumentNotNull(options, nameof(options));

            _defaultDatabaseProvider = new Lazy<IDatabaseProvider>(() => (IDatabaseProvider)Activator.CreateInstance(options.Value.DefaultDatabaseProvider));
            metadataProviders = metadataProviders ?? Enumerable.Empty<IDapperMetadataProvider>();
            _mappedDataSources = new ConcurrentDictionary<Type, DapperDataSource>();
            _databaseProviders = new ConcurrentDictionary<String, IDatabaseProvider>();
            _options = options;
            var metadatas = metadataProviders.Select(m => m.GetMetadata()).ToList();
            try
            {
                _metadataSet = new ConcurrentDictionary<Type, DapperMetadata>(metadatas.ToDictionary(m => m.EntityType));
            }
            catch (ArgumentException exception)
            {
                var duplicate = metadatas.FirstOrDefault(metadata => metadatas.Count(item => item.EntityType == metadata.EntityType) > 1);
                throw new SchubertException($"{duplicate?.EntityType} 存在重复的 IDapperMetadataProvider ,请检查", exception);
            }
            _crudSegments = new ConcurrentDictionary<Type, CrudSqlSegments>();
        }

        /// <summary>
        /// 根据数据库连接字符串名称获取数据库提供程序。
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public IDatabaseProvider GetDatabaseProvider(String connectionName = null)
        {
            if (connectionName.IsNullOrWhiteSpace())
            {
                return _defaultDatabaseProvider.Value;
            }
            return _databaseProviders.GetOrAdd(connectionName, c =>
            {
                if (_options.Value.DatabaseProviders.TryGetValue(c, out Type provider))
                {
                    return (IDatabaseProvider)Activator.CreateInstance(provider);
                }
                else
                {
                    return _defaultDatabaseProvider.Value;
                }
            });
        }

        /// <summary>
        /// 获取指定类型对应的数据源。
        /// </summary>
        public DapperDataSource GetDataSource(Type entityType, bool throwIfNotFound = true)
        {
            return this._mappedDataSources.GetOrAdd(entityType, et =>
            {
                DapperMetadata metadata = this.GetMetadata(et, throwIfNotFound);
                return NewDataSource(metadata?.DbReadingConnectionName,
                    metadata?.DbWritingConnectionName);
            });
        }

        private DapperDataSource NewDataSource(string readingConnectionName, string writingConnectionName)
        {
            var wprovider = this.GetDatabaseProvider(readingConnectionName);
            var rprovider = this.GetDatabaseProvider(writingConnectionName);
            if (wprovider.GetType() != rprovider.GetType())
            {
                throw new SchubertException($"数据库连接 {writingConnectionName} 和 {readingConnectionName} 配置了不同的 {nameof(IDatabaseProvider)}，读写库必须使用一致的提供程序。");
            }
            return new DapperDataSource(wprovider, readingConnectionName, writingConnectionName);
        }

        /// <summary>
        /// 根据实体类型获取 Dapper 配置的元数据。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="throwIfNotFound">未发现元数据时是否抛异常</param>
        /// <returns></returns>
        public DapperMetadata GetMetadata(Type entityType, bool throwIfNotFound = true)
        {
            if (_metadataSet.TryGetValue(entityType, out DapperMetadata value))
            {
                return value;
            }
            var convention = _options.Value.GetConvention();
            var item = convention.TypeConventions.FirstOrDefault(x => x.Filter(entityType) && x.Mapped);
            if (item == null && throwIfNotFound)
            {
                throw new InvalidOperationException(
                    $"找不到类型 {entityType.Name} 的元数据提供程序， 请确保已经通过 {typeof(DapperMetadataProvider<>).Name} 或者{typeof(TypeConvention).Name}实现了元数据提供程序。");
            }
            value = new DapperMetadata(entityType, item, convention);
            _metadataSet.TryAdd(entityType, value);
            return value;
        }

        /// <summary>
        /// 获取当前  Dapper 运行环境下的 SQL 生成器。
        /// </summary>
        public DapperSqlGenerator SqlGenerator
        {
            get { return _sqlGenerator ?? (_sqlGenerator = new DapperSqlGenerator(this)); }
        }

        /// <summary>
        /// 根据实体类型获取基本的T-SQL语句。
        /// </summary>
        /// <param name="entityType">要用于CRUD的实体类型。</param>
        /// <returns></returns>
        public CrudSqlSegments GetCrudSegments(Type entityType)
        {
            CrudSqlSegments sql = _crudSegments.GetOrAdd(entityType, t => new CrudSqlSegments(t, this));
            return sql;
        }

        /// <summary>
        /// 获取数据库连接字符串。
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public string GetDbConnectionString(string connectionName = null)
        {
            return _options.Value.GetConnectionString(connectionName);
        }

        /// <summary>
        /// 使用界定符包围T-SQL中的主体字符串。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="identifier">要进行包围的主体字符。</param>
        /// <returns></returns>
        public string DelimitIdentifier(Type entityType, string identifier)
        {
            var dataSource = this.GetDataSource(entityType);
            return $"{dataSource.DatabaseProvider.IdentifierPrefix}{identifier}{dataSource.DatabaseProvider.IdentifierStuffix}";
        }

        /// <summary>
        /// 使用界定符包围T-SQL中的参数字符串。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="parameter">要进行包围的参数字符。</param>
        /// <returns></returns>
        public string DelimitParameter(Type entityType, string parameter)
        {
            var dataSource = this.GetDataSource(entityType);
            return $"{dataSource.DatabaseProvider.ParameterPrefix}{parameter}";
        }
    }
}
