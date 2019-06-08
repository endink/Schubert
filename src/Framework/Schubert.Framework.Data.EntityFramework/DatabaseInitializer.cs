using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Schubert.Framework.Data.Mappings;
using Schubert.Framework.Environment;
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
    public class DatabaseInitializer
    {
        private ConcurrentDictionary<Type, SyncCreation> _contextCreationSync = null;
        private DbOptions _dbOptions;

        private class SyncCreation
        {
            public object SyncRoot { get; } = new object();

            public bool Checked { get; set; }
        }

        public static readonly DatabaseInitializer Default = new DatabaseInitializer();

        private DbOptions GetDbOptions(DbContext context)
        {
            if (_dbOptions == null)
            {
                _dbOptions = context.GetDbOptions() ?? throw new SchubertException("IOptions<DbOptions> cant not be null.");
            }
            return _dbOptions;
        }

        private DatabaseInitializer() {
            _contextCreationSync = new ConcurrentDictionary<Type, SyncCreation>();
        }

        private void SkipMigrations(DbContext dbContext)
        {
            IHistoryRepository historyRepository = dbContext.Database.GetService<IHistoryRepository>();
            IMigrationsAssembly migrationAssembly = dbContext.Database.GetService<IMigrationsAssembly>();
            //IMigrationsAssembly migrationAssembly = (IMigrationsAssembly)ActivatorUtilities.CreateInstance(provider, typeof(MigrationFinder), _shellContext);
            var ids = migrationAssembly.Migrations.Keys;

            StringBuilder builder = new StringBuilder();
            var options = GetDbOptions(dbContext);
            bool exist = options.GetDbProvider(dbContext.GetType()).ExistTables(options, dbContext, new string[] { HistoryRepository.DefaultTableName });
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
            var sync = _contextCreationSync.GetOrAdd(dbContextType, new SyncCreation());
            if (!sync.Checked)
            {
                lock (sync.SyncRoot)
                {
                    if (!sync.Checked)
                    {
                        var database = context.Database;

                        var options = GetDbOptions(context);
                        var provider = options.GetDbProvider(dbContextType);
                        var tables = options.GetTablesForChecking(dbContextType);
                        if (options.CreateDatabaseIfNotExisting)
                        {
                            if (tables.IsNullOrEmpty())
                            {
                                throw new SchubertException("CreateDatabaseIfNotExisting 配置为 true （默认值）， 必须使用 TablesForCheckingDatabaseExists 来设置用于检查数据库的表。");
                            }
                            if (!context.GetType().Equals(typeof(ShellDescriptorDbContext)))
                            {
                                bool created = database.EnsureCreated();
                                var providerServices = database.GetService<IRelationalDatabaseCreator>();
                                bool existedTables = provider.ExistTables(options, context, tables);
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
    }
}
