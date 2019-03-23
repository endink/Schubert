using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Schubert.Framework.Data.Conventions;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 表示 Dapper 的元数据。
    /// </summary>
    public class DapperMetadata
    {
        public DapperMetadata(Type entityType)
        {
            Guard.ArgumentNotNull(entityType, nameof(entityType));

            this.EntityType = entityType;
            this.Fields = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && IsFieldType(p.PropertyType))
                .Select(p => new DapperFieldMetadata(p)).ToArray();
            this.TableName = MappingStrategyParser.Parse(entityType.Name);
        }

        public DapperMetadata(Type entityType, TypeConvention typeConvention, ModelConvention modelConvention)
        {
            Guard.ArgumentNotNull(entityType, nameof(entityType));
            this.EntityType = entityType;

            var metadatas = from prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            where prop.CanRead && prop.CanWrite && IsFieldType(prop.PropertyType)
                            let convention = modelConvention.PropertyConventions.FirstOrDefault(x => x.Filter(prop))
                            select (convention != null ? new DapperFieldMetadata(prop, convention) : new DapperFieldMetadata(prop));


            this.Fields = metadatas.ToArray();
            this.TableName = MappingStrategyParser.Parse(entityType.Name);


            if (typeConvention.Filter(entityType))
            {
                this.DbReadingConnectionName = typeConvention.DbReadingConnectionName;
                this.DbWritingConnectionName = typeConvention.DbWritingConnectionName;
            }
        }
        private static bool IsFieldType(Type type)
        {
            return type == typeof(string) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(Guid) ||
                type == typeof(byte[]) ||
                type == typeof(decimal) ||
                type == typeof(char) ||
                type == typeof(bool) ||
                type == typeof(byte) ||
                type == typeof(DateTime) ||
                type == typeof(TimeSpan) ||
                type.GetTypeInfo().IsEnum ||
                (Nullable.GetUnderlyingType(type) != null && IsFieldType(Nullable.GetUnderlyingType(type)));
        }

        public Type EntityType { get; }

        public string TableName { get; internal set; }

        public IEnumerable<DapperFieldMetadata> Fields { get; }

        public String DbReadingConnectionName { get; internal set; }

        public String DbWritingConnectionName { get; internal set; }
    }
}
