using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Schubert.Framework.Data;
using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert
{
    public static class DbUtility
    {
        //public static string GetDbIndentity(this DbContext dbContext)
        //{
        //    var dbConnection = dbContext.Database.GetDbConnection();
        //    string dataSource = dbConnection.DataSource == "." ? "(local)" : dbConnection.DataSource;
        //    dataSource = dataSource.ToLower().Replace("127.0.0.1", "(local)");
        //    return $"{dataSource}/{dbConnection.Database.ToLower()}";
        //}

         /// <summary>
         /// 以指定的数据库提供程序配置文本字段的长度。
         /// 该配置针对 mysql 会自动根据长度的不同完成 varchar、text 等映射。
         /// </summary>
         /// <param name="pb"></param>
         /// <param name="length">要存储的字符串长度。</param>
         /// <param name="dbProvider">数据库提供程序。</param>
         /// <returns></returns>
        public static PropertyBuilder<String> HasMaxLength(this PropertyBuilder<String> pb, int length, IDbProvider dbProvider)
        {
            Guard.ArgumentNotNull(dbProvider, nameof(dbProvider));
            if (length > 0)
            {
                return dbProvider.StringColumnLength(pb, length);
            }
            return pb;
        }

        public static void DetachEntities(this DbContext context, Func<EntityEntry, bool> predicate)
        {
            var entites = context.ChangeTracker.Entries().ToArray();
            foreach (var e in entites)
            {
                if (predicate?.Invoke(e) ?? true)
                {
                    e.State = EntityState.Detached;
                }
            }
        }

        public static IServiceProvider GetApplicationServiceProvider(this DbContext context)
        {
            IServiceProvider serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;
            var options = serviceProvider.GetRequiredService<IDbContextOptions>();
            var applicationServiceProvider = options.FindExtension<CoreOptionsExtension>().ApplicationServiceProvider;
            return applicationServiceProvider;
        }

        public static DbOptions GetDbOptions(this DbContext context)
        {
            var applicationServiceProvider = context.GetApplicationServiceProvider();
            
            return applicationServiceProvider.GetRequiredService<IOptions<DbOptions>>().Value;
        }

        public static IDbProvider GetDbProvider(this DbContext context)
        {
            return context.GetDbOptions().GetDbProvider(context.GetType());
        }

        public static IDatabaseTransaction BeginTransaction(this DbContext dbContext, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            var tran = dbContext.Database.BeginTransaction(level);
            return new DbContextTransactionWrapper(tran);
        }

        public static void Migrate(this DbContext dbContext)
        {
            dbContext.Database.Migrate();
        }

        public static Task MigrateAsync(this DbContext dbContext)
        {
            return dbContext.Database.MigrateAsync();
        }
    }
}
