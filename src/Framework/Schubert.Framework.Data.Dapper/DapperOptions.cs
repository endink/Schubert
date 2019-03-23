using Schubert.Framework.Data.Conventions;
using System;
using System.Collections.Generic;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 大小写规则
    /// </summary>
    public enum CapitalizationRule
    {
        /// <summary>
        /// 小写
        /// </summary>
        LowerCase = 0,
        /// <summary>
        /// 大写
        /// </summary>
        UpperCase = 1,
        /// <summary>
        /// 原串
        /// </summary>
        Original = 2
    }

    public class DapperOptions
    {
        private IdentifierMappingStrategy _identifierMappingStrategy
            = IdentifierMappingStrategy.Underline;
        private CapitalizationRule _capitalizationRule
           = CapitalizationRule.LowerCase;
        /// <summary>
        /// 表名映射策略
        /// </summary>
        public virtual IdentifierMappingStrategy IdentifierMappingStrategy
        {
            get { return _identifierMappingStrategy; }
            set
            {
                _identifierMappingStrategy = value;
                if (_identifierMappingStrategy == IdentifierMappingStrategy.PascalCase)
                {
                    global::Dapper.DefaultTypeMap.MatchNamesWithUnderscores = false;
                }
                else
                {
                    global::Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                }
            }
        }

        public CapitalizationRule CapitalizationRule
        {
            get { return _capitalizationRule; }
            set
            {
                _capitalizationRule = value;
            }
        }

    }

    public class DapperDatabaseOptions : DatabaseOptions
    {
        private DapperOptions _dapperOptions;
        private Type _databaseProvider;
        private Lazy<ModelConvention> _convention;

        public DapperDatabaseOptions()
        {
            _convention = new Lazy<ModelConvention>(() =>
            {
                ModelConvention c = new ModelConvention();
                this.ConventionConfigure?.Invoke(c);
                return c;
            }, false);
        }


        internal Action<ModelConvention> ConventionConfigure { get; set; }

        internal Dictionary<String, Type> DatabaseProviders { get; set; } = new Dictionary<string, Type>();

        public virtual DapperOptions Dapper
        {
            get
            {
                return _dapperOptions ?? (_dapperOptions = new DapperOptions());
            }
            set
            {
                _dapperOptions = value;
            }
        }

        public Type DefaultDatabaseProvider
        {
            get { return this._databaseProvider ?? (_databaseProvider = typeof(MySqlDatabaseProvider)); }
            set
            {
                if (value != null)
                {
                    Guard.TypeIsAssignableFromType(value, typeof(IDatabaseProvider), "DefaultDatabaseProvider");
                }
                _databaseProvider = value;
            }
        }


        internal ModelConvention GetConvention()
        {
            return _convention.Value;
        }
    }
}
