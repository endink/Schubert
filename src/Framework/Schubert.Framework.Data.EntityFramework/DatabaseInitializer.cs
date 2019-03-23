using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Schubert.Framework.Data.Mappings;
using Schubert.Framework.Environment.ShellBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Data.EntityFramework
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private DbOptions _options;
        private IEnumerable<IEntityMappings> _entityMappings = null;
        private ILogger _logger = null;
        private ILoggerFactory _loggerFactory;
        private ShellContext _shellContext = null;
        private ConcurrentDictionary<Type, SyncCreation> _contextCreationSync = null;

        private class SyncCreation
        {
            public object SyncRoot { get; } = new object();

            public bool Checked { get; set; }
        }

        public DatabaseInitializer(
            ShellContext shellContext,
            IOptions<DbOptions> options,
            IEnumerable<IEntityMappings> mappings,
            ILoggerFactory loggerFactory)
        {
            Guard.ArgumentNotNull(loggerFactory, nameof(loggerFactory));
            Guard.ArgumentNotNull(options, nameof(options));
            Guard.ArgumentNotNull(mappings, nameof(mappings));
            Guard.ArgumentNotNull(shellContext, nameof(shellContext));

            _contextCreationSync = new ConcurrentDictionary<Type, SyncCreation>();
            _logger = loggerFactory?.CreateLogger<DatabaseInitializer>() ?? (ILogger)NullLogger.Instance;
            _entityMappings = mappings;
            _options = options.Value;
            _shellContext = shellContext;
            _loggerFactory = loggerFactory;

        }

        private void SkipMigrations(DbContext dbContext)
        {
            IServiceProvider provider = ((IInfrastructure<IServiceProvider>)dbContext).Instance;
            IHistoryRepository historyRepository = dbContext.Database.GetService<IHistoryRepository>();
            IMigrationsAssembly migrationAssembly = dbContext.Database.GetService<IMigrationsAssembly>();
            //IMigrationsAssembly migrationAssembly = (IMigrationsAssembly)ActivatorUtilities.CreateInstance(provider, typeof(MigrationFinder), _shellContext);
            var ids = migrationAssembly.Migrations.Keys;

            StringBuilder builder = new StringBuilder();
            bool exist = _options.GetDbProvider(dbContext.GetType()).ExistTables(_options, dbContext, new string[] { HistoryRepository.DefaultTableName });
            if (!exist)
            {
                builder.AppendLine(historyRepository.GetCreateScript());
            }
            //builder.AppendLine(historyRepository.GetCreateIfNotExistsScript());
            foreach (string id in ids)
            {
                string sql = historyRepository.GetInsertScript(new HistoryRow(id, typeof(DbContext).GetTypeInfo().Assembly.GetName().Version.ToString()));
                builder.AppendLine(sql);
            }
            if (builder.Length > 0)
            {
                dbContext.Database.ExecuteSqlCommand(builder.ToString());
            }
        }

        public void InitializeContext<T>(T context)
            where T : DbContext, INewDatabaseFlag
        {
            Type dbContextType = context.GetType();
            var database = context.Database;
            var provider = _options.GetDbProvider(dbContextType);
            var tables = _options.GetTablesForChecking(dbContextType);
            if (_options.CreateDatabaseIfNotExisting)
            {
                if (tables.IsNullOrEmpty())
                {
                    throw new SchubertException("CreateDatabaseIfNotExisting 配置为 true （默认值）， 必须使用 TablesForCheckingDatabaseExists 来设置用于检查数据库的表。");
                }
                var sync = _contextCreationSync.GetOrAdd(dbContextType, new SyncCreation());
                if (!sync.Checked)
                {
                    lock (sync.SyncRoot)
                    {
                        if (!sync.Checked)
                        {
                            if (!context.GetType().Equals(typeof(ShellDescriptorDbContext)))
                            {
                                bool created = database.EnsureCreated();
                                var providerServices = database.GetService<IRelationalDatabaseCreator>();
                                bool existedTables = provider.ExistTables(_options, context, tables);
                                if (!existedTables)
                                {
                                    providerServices.CreateTables();
                                }
                                context.IsNew = (created || !existedTables);
                                if (context.IsNew)
                                {
                                    SkipMigrations(context);
                                }
                            }

                            sync.Checked = true;
                        }
                    }
                }
            }
        }

        public void CreateModel(ModelBuilder builder, DbContext context)
        {
            var contextType = context.GetType();

            var dbProvider = _options.GetDbProvider(contextType);
            dbProvider.OnCreateModel(builder, _options); 

            //当 Shell 没创建时候不会产生任何的 Mapping
            foreach (var mapping in _entityMappings)
            {
                DbContextSelectionAttribute att = mapping.GetType().GetAttribute<DbContextSelectionAttribute>();
                DbContextAttribute att2 = mapping.GetType().GetAttribute<DbContextAttribute>();
                if ((att == null && att2 == null && contextType.Equals(_options.DefaultDbContext)) || (att?.ContextType == contextType || att2?.ContextType == contextType))
                {
                    mapping.ApplyMapping(builder, dbProvider);
                }
            }


            bool mapBuiltinEntities = _options.IncludeBuiltinEntities(contextType);
            if (mapBuiltinEntities)
            {
                foreach (var mapping in this.GetBuiltinMappings())
                {
                    mapping.ApplyMapping(builder, dbProvider);
                }
            }
        }

        /// <summary>
        /// 获取 Schubert 框架内置的映射。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IEntityMappings> GetBuiltinMappings()
        {
            yield return new LanguageMapping();
            yield return new PermissionMapping();
            yield return new PermissionRoleMapping();
            yield return new StringResourceMapping();
        }
    }
}
