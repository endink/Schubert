using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schubert.Framework.Environment;
using Schubert.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public sealed class DbBuilder
    {
        public DbBuilder()
        {
            this.DbConfigurings = new Dictionary<Type, Action<IServiceCollection>>();
            this.DbSettings = new Dictionary<Type, DbContextSettings>();
            this.DbContexts = new Dictionary<string, Type>();
        }
        
        internal Dictionary<Type, Action<IServiceCollection>> DbConfigurings { get; }

        internal Dictionary<String, Type> DbContexts { get; }

        internal Action<DbOptions> Setup { get; set; }

        internal Dictionary<Type, DbContextSettings> DbSettings { get; set; }

        internal Type DefaultDbContext { get; set; }

        internal String DefaultDbConnectionName { get; set; }

        internal bool ShellDbAdded { get; set; }

      
        private DbBuilder AddShellDbContext(String dbConnectionName, String connectionString, IDbProvider provider)
        {
            if (!ShellDbAdded)
            {
                ShellDbAdded = true;
                AddDbContext<ShellDescriptorDbContext>(dbConnectionName, connectionString, s =>
                {
                    s.Provider(provider);
                    s.MigrationAssembly(typeof(ShellDescriptorDbContext).GetTypeInfo().Assembly.FullName);
                });
                this.DbConfigurings[provider.GetType()] = services => provider.OnAddDependencies(services);
            }
            return this;
        }

        private DbBuilder AddDbContext<TContext>(String dbConnectionName, String connectionString, Action<DbContextSettingsBuilder> setup)
            where TContext : DbContext, INewDatabaseFlag
        {
            if (dbConnectionName.IsNullOrWhiteSpace() && connectionString.IsNullOrWhiteSpace())
            {
                throw new ArgumentException($"必须提供非空的 {nameof(dbConnectionName)} 或 {nameof(connectionString)}。");
            }
            String connName = dbConnectionName;
            if (connName.IsNullOrWhiteSpace())
            {
                connName = Guid.NewGuid().ToString();
                this.Setup += ((DbOptions innerOptions) =>
                {
                    innerOptions.ConnectionStrings.Add(connName, connectionString);
                });
            }
            this.DbContexts[connName] = typeof(TContext);

            DbContextSettingsBuilder builder = new DbContextSettingsBuilder(connName, connectionString);
            setup?.Invoke(builder);

            SetDefaultDbContext<TContext>(builder);

            DbContextSettings settings = builder.Build();

            this.DbSettings[typeof(TContext)] = settings;
            if (!this.DbConfigurings.ContainsKey(settings.DbProvider.GetType()))
            {
                this.DbConfigurings[settings.DbProvider.GetType()] = services => settings.DbProvider.OnAddDependencies(services);
            }

            this.DbConfigurings[typeof(TContext)] = c =>
            {

                c.AddDbContext<TContext>(optionsAction: (sp, b) =>
                {
                    var options = sp.GetRequiredService<IOptions<DbOptions>>();
                    settings.DbProvider.OnBuildContext(typeof(TContext), b, options.Value);
                    b.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
                });
            };

            if (builder.IsDefaultDbContext)
            {
                this.DbConfigurings[typeof(TContext)] += services =>
                {
                    services.AddScoped(typeof(DbContext), s => (DbContext)s.GetRequiredService(typeof(TContext)));
                };
            }
            return this;
        }

        private void SetDefaultDbContext<TContext>(DbContextSettingsBuilder builder) where TContext : DbContext, INewDatabaseFlag
        {
            if (builder.IsDefaultDbContext)
            {
                if (DefaultDbContext == null)
                {
                    this.DefaultDbContext = typeof(TContext);
                    this.DefaultDbConnectionName = builder.ConnectionName;
                }
                else
                {
                    throw new ArgumentException($"已指定了默认的 DbContext 类型 '{this.DefaultDbContext.FullName}', 不能指定多个默认的DbContext。");
                }
            }
        }

        /// <summary>
        /// 创建 Shell上下文，包括DB配置支持和 ShellManager 支持。
        /// </summary>
        /// <param name="dbConnectionName">数据库连接字符串的名称（连接字符串通过 <see cref="Schubert.Framework.Data.DatabaseOptions"/> 配置）。</param>
        /// <param name="provider"></param>
        public DbBuilder AddShellDbContext(String dbConnectionName, IDbProvider provider)
        {
            return this.AddShellDbContext(dbConnectionName, null, provider);
        }

        /// <summary>
        /// 创建 Shell上下文，包括DB配置支持和 ShellManager 支持。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <param name="provider"></param>
        public DbBuilder AddShellDbContextWithConnectionString(String connectionString, IDbProvider provider)
        {
            return this.AddShellDbContext(null, connectionString, provider);
        }

        /// <summary>
        /// 添加 EntityFramework 的数据库上下文。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <param name="setup">配置数据库行为。</param>
        public DbBuilder AddDbContextWithConnectionString<TContext>(String connectionString, Action<DbContextSettingsBuilder> setup = null)
            where TContext : DbContext, INewDatabaseFlag
        {
            return this.AddDbContext<TContext>(null, connectionString, setup);
        }

        /// <summary>
        /// 添加 EntityFramework 的数据库上下文。
        /// </summary>
        /// <param name="dbConnectionName">数据库连接字符串的名称（连接字符串通过 <see cref="Schubert.Framework.Data.DatabaseOptions"/> 配置）。</param>
        /// <param name="setup">配置数据库行为。</param>
        public DbBuilder AddDbContext<TContext>(String dbConnectionName, Action<DbContextSettingsBuilder> setup = null)
            where TContext : DbContext, INewDatabaseFlag
        {
            return this.AddDbContext<TContext>(dbConnectionName, null, setup);
        }


        /// <summary>
        /// 配置数据库全局选项。
        /// </summary>
        /// <param name="setup"></param>
        public void ConfigureGlobal(Action<DbOptions> setup)
        {
            this.Setup += setup;
        }
    }
}
