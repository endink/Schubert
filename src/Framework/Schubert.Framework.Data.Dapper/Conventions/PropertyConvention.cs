using System;
using System.Reflection;

namespace Schubert.Framework.Data.Conventions
{
    public class PropertyConvention
    {
        internal PropertyConvention(Func<PropertyInfo, bool> filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));
            this.Filter = filter;
            ColumnRule = DbColumnRule.None;
        }

        internal Func<PropertyInfo, bool> Filter { get; }
        internal DbColumnRule ColumnRule { get; private set; }

        /// <summary>
        /// 指示该属性是一个数据库的主键。
        /// </summary>
        /// <returns></returns>
        public void IsKey()
        {
            this.ColumnRule = DbColumnRule.Key;
        }
        public void AutoGeneration()
        {
            this.ColumnRule = DbColumnRule.AutoGeneration;
        }

        public void AutoGenerateKey()
        {
            this.ColumnRule = DbColumnRule.AutoGeneration | DbColumnRule.Key;
        }

        /// <summary>
        /// 指示该属性不映射到数据库。
        /// </summary>
        /// <returns></returns>
        public void Ignore()
        {
            this.ColumnRule = DbColumnRule.Ignore;
        }
        [Flags]
        public enum DbColumnRule : byte
        {
            None = 0x00,
            Ignore = 0x02,
            Key = 0x04,
            AutoGeneration = 0x08
        }
    }
}
