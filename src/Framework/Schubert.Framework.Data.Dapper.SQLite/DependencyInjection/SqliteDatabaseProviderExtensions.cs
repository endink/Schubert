using Schubert.Framework.Data.Dapper.SQLite;
using Schubert.Framework.Data.DependencyInjection;

namespace Schubert
{
    public static class SQLiteDatabaseProviderExtensions
    {
        /// <summary>
        /// 将 SQLite 作为默认的数据库提供程序。
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static DapperDataFeatureBuilder AddSQLiteForDefault(this DapperDataFeatureBuilder builder)
        {
            builder.Options(options =>
            {
                options.DefaultDatabaseProvider = typeof(SQLiteDatabaseProvider);
            });
            return builder;
        }
    }
}
