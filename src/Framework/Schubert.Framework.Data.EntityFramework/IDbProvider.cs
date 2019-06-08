using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 表示数据提供程序。
    /// </summary>
    public interface IDbProvider
    {
        /// <summary>
        /// 表示主体字段的前缀。
        /// </summary>
        string IdentifierPrefix { get; }

        /// <summary>
        /// 表示主体字段的后缀。
        /// </summary>
        string IdentifierStuffix { get; }
        /// <summary>
        /// 参数前缀
        /// </summary>
        string ParameterPrefix { get; }

        void OnAddDependencies(IServiceCollection serviceCollection);

        void OnBuildContext(Type dbContextType, DbContextOptionsBuilder builder, DbOptions options);

        void OnCreateModel(ModelBuilder builder, DbOptions options);

        bool ExistTables(DbOptions dbOptions, DbContext dbContext, IEnumerable<String> tableNames);

        PropertyBuilder<String> StringColumnLength(PropertyBuilder<String> pb, int length);

        bool HasUniqueConstraintViolation(DbUpdateException ex);

        DbParameter CreateDbParameter(string name, object value);
    }
}
