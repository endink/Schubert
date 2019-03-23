using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Schubert.Framework.Environment.ShellBuilders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Schubert.Framework.Data
{
    /// <summary>
    ///  为模块化数据定义提供 EntityFramework 支持（业务开发人员无需关心该类）。
    /// </summary>
    public sealed class MigrationFinder : MigrationsAssembly
    {
        private ShellContext _shellContext = null;
        private LazyRef<IReadOnlyDictionary<string, TypeInfo>> _migrations = null;
        private IServiceProvider _serviceProvider;

        public MigrationFinder(IServiceProvider serviceProvider, ShellContext shellContext, ICurrentDbContext context, IDbContextOptions options, 
            IMigrationsIdGenerator migrationsIdGenerator, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
            : base(context, options, migrationsIdGenerator, logger)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            _shellContext = shellContext;
            _serviceProvider = serviceProvider;
            var contextType = context.Context.GetType();
            _migrations = new LazyRef<IReadOnlyDictionary<string, TypeInfo>>(
                () =>
                {
                    var dic = (
                        from t in _shellContext.Blueprint.Dependencies.Select(d => d.Type.GetTypeInfo())
                        where t.IsSubclassOf(typeof(Migration))
                            && t.GetCustomAttribute<MigrationAttribute>() != null
                            && this.CanApply(t, contextType)
                        let id = t.GetCustomAttribute<MigrationAttribute>()?.Id
                        orderby id
                        select new { Key = id, Element = t })
                    .ToDictionary(i => i.Key, i => i.Element);

                    return new ReadOnlyDictionary<string, TypeInfo>(dic);
                });
        }

        public override string FindMigrationId(string nameOrId)
        {
            return base.FindMigrationId(nameOrId);
        }

        public override Assembly Assembly
        {
            get
            {
                return base.Assembly;
            }
        }

        private bool CanApply(TypeInfo migrationType, Type contextType)
        {
            DbContextAttribute att1 = migrationType.GetCustomAttribute<DbContextAttribute>();
            DbContextSelectionAttribute att2 = migrationType.GetCustomAttribute<DbContextSelectionAttribute>();

            return (att1 == null && att2 == null) || (att1?.ContextType == contextType || att2?.ContextType == contextType);
        }

        public override Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
        {
            return (Migration)_serviceProvider.GetService(migrationClass.AsType());
        }

        public override IReadOnlyDictionary<string, TypeInfo> Migrations
        {
            get
            {
                return _migrations.Value;
            }
        }
    }
}
