using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data.Conventions
{
    public class TypeConvention
    {
        internal TypeConvention(Func<Type, bool> filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));
            this.Filter = filter;
        }

        internal Func<Type, bool> Filter { get; }
        internal bool Mapped { get; private set; }


        internal String DbReadingConnectionName { get; set; }

        internal String DbWritingConnectionName { get; set; }

        /// <summary>
        /// 指示该类型是一个实体。
        /// </summary>
        /// <returns></returns>
        public TypeConvention IsEntity()
        {
            return this.IsEntity(null, null);
        }

        /// <summary>
        /// 指示该类型是一个实体。
        /// </summary>
        /// <param name="dbConnectionName">实体使用的数据库连接字符串名称。</param>
        /// <returns></returns>
        public TypeConvention IsEntity(String dbConnectionName)
        {
            return this.IsEntity(dbConnectionName, dbConnectionName);
        }

        /// <summary>
        /// 指示该类型是一个实体。
        /// </summary>
        /// <param name="writingConnectionName">实体使用的写数据库连接字符串名称。</param>
        /// <param name="readingConnectionName">实体使用的读数据库连接字符串名称。</param>
        /// <returns></returns>
        public TypeConvention IsEntity(String writingConnectionName, String readingConnectionName)
        {
            this.DbWritingConnectionName = writingConnectionName;
            this.DbReadingConnectionName = readingConnectionName;
            this.Mapped = true;
            return this;
        }

    }
}
