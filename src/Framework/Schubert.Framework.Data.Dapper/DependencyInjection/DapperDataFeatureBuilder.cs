using Schubert.Framework.Data.Conventions;
using System;
using System.Collections.Generic;

namespace Schubert.Framework.Data.DependencyInjection
{
    public sealed class DapperDataFeatureBuilder
    {
        private Action<DapperDatabaseOptions> _configure;
        private Action<ModelConvention> _convention;
        private readonly DapperDatabaseOptions _options;


        public DapperDataFeatureBuilder(DapperDatabaseOptions options = null)
        {
            this.DataProviders = new Dictionary<string, Type>();
            _options = options;
        }

        public void Build()
        {
            if (_options != null)
            {
                this.Configure(_options);
            }
        }

        internal Action<DapperDatabaseOptions> Configure
        {
            get
            {
                return op =>
                {
                    _configure?.Invoke(op);
                    op.ConventionConfigure = _convention;
                    op.DatabaseProviders = this.DataProviders;
                };
            }
        }

        internal Dictionary<string, Type> DataProviders { get; set; }

        /// <summary>
        /// 配置 Dapper 选项。
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public DapperDataFeatureBuilder Options(Action<DapperDatabaseOptions> configure)
        {
            _configure = configure;
            return this;
        }

        /// <summary>
        /// 映射数据库连接字符串到数据库提供程序。
        /// </summary>
        /// <param name="providerName">数据库连接字符串名称（注意，ConnectionStrings 中配置的名称）。</param>
        /// <param name="databaseProvider">数据库提供程序（必须是 <see cref="IDatabaseProvider"/> 实现类型，切具有公共无参构造函数）。</param>
        /// <returns></returns>
        public DapperDataFeatureBuilder MapDatabaseProvider(String providerName, Type databaseProvider)
        {
            Guard.ArgumentNullOrWhiteSpaceString(providerName, nameof(providerName));
            Guard.TypeIsAssignableFromType(databaseProvider, typeof(IDatabaseProvider), nameof(databaseProvider));

            this.DataProviders[providerName] = databaseProvider;
            return this;
        }

        /// <summary>
        /// 设置数据库模型（映射）约定。
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public DapperDataFeatureBuilder Conventions(Action<ModelConvention> configure)
        {
            if (_convention == null)
            {
                _convention = configure;
                return this;
            }
            _convention += configure;
            return this;
        }
    }
}
