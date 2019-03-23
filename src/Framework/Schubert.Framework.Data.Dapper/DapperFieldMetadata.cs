using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Schubert.Framework.Data.Conventions;

namespace Schubert.Framework.Data
{
    public class DapperFieldMetadata
    {
        internal DapperFieldMetadata(PropertyInfo field)
        {
            this.Field = field;
            this.Name = MappingStrategyParser.Parse(field.Name);
        }

        internal DapperFieldMetadata(PropertyInfo field, PropertyConvention convention)
        {
            this.Field = field;
            this.Name = MappingStrategyParser.Parse(field.Name);
            var rule = convention.ColumnRule;
            var valueOf = new Func<PropertyConvention.DbColumnRule, PropertyConvention.DbColumnRule, bool>((x, y) => (x & y) == y);
            AutoGeneration = valueOf(rule, PropertyConvention.DbColumnRule.AutoGeneration);
            IsKey = valueOf(rule, PropertyConvention.DbColumnRule.Key);
            Ignore = valueOf(rule, PropertyConvention.DbColumnRule.Ignore);
        }

        public PropertyInfo Field { get; }

        public string Name { get; }

        public bool AutoGeneration { get; set; }

        public bool IsKey { get; set; }

        public bool Ignore { get; set; }
    }
}
