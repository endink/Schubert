using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 实体映射构造器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DapperMetadataBuilder<T>
        where T : class
    {
        private DapperMetadata _mapping = null;
        public DapperMetadataBuilder()
        {
            _mapping = new DapperMetadata(typeof(T));
        }

        public DapperMetadataBuilder<T> TableName(string tableName)
        {
            Guard.ArgumentNullOrWhiteSpaceString(tableName, nameof(tableName));
            _mapping.TableName = tableName;
            return this;
        }



        /// <summary>
        /// 指示实体的某个属性是通过DB自动生成。
        /// </summary>
        /// <param name="expression">表示实体属性的表达式（示例：a=>a.P ）。</param>
        /// <returns></returns>
        public DapperMetadataBuilder<T> AutoGeneration(Expression<Func<T, Object>> expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            Expression visited = expression.Body;
            UnaryExpression unary = visited as UnaryExpression;
            if (unary != null)
            {
                visited = unary.Operand;
            }

            MemberExpression mex = visited as MemberExpression;
            if (mex != null)
            {
                string name = MappingStrategyParser.Parse(mex.Member.Name);
                var foundField = _mapping.Fields.FirstOrDefault(k => k.Name.CaseInsensitiveEquals(name));
                if (foundField != null)
                {
                    foundField.AutoGeneration = true;
                }
            }
            else
            {
                throw new ArgumentException($@"{nameof(DapperMetadataBuilder<T>)}.{nameof(AutoGeneration)} 
                                                                        方法只接受单个属性表达式，例如 a=>a.Property。");
            }
            return this;
        }

        /// <summary>
        /// 指定实体的键。
        /// </summary>
        /// <param name="expression">表示实体键的表达式（示例：a=>a.P ，当需要复合键时使用 a=>new { a.P1, a.P2 }）。</param>
        /// <returns></returns> 
        public DapperMetadataBuilder<T> HasKey(Expression<Func<T, Object>> expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            foreach (var f in _mapping.Fields)
            {
                f.IsKey = false;
            }

            Expression visited = expression.Body;
            UnaryExpression unary = visited as UnaryExpression;
            if (unary != null)
            {
                visited = unary.Operand;
            }

            MemberExpression mex = visited as MemberExpression;
            if (mex != null)
            {
                string name = MappingStrategyParser.Parse(mex.Member.Name);
                var keyField = _mapping.Fields.FirstOrDefault(p => p.Name.CaseInsensitiveEquals(name));
                if (keyField == null)
                {
                    throw new ArgumentException($"{nameof(DapperMetadataBuilder<T>)}.{nameof(DapperMetadataBuilder<T>.HasKey)} 方法传入的属性表达式找不到属性 {keyField}。");
                }
                if (keyField.Ignore)
                {
                    throw new ArgumentException($"{nameof(DapperMetadataBuilder<T>)}.{nameof(DapperMetadataBuilder<T>.HasKey)} 不能指定一个已忽略的属性作为 Key 。");
                }
                keyField.IsKey = true;
                return this;
            }

            NewExpression newExpression = visited as NewExpression;
            if (newExpression != null)
            {
                var propertyNames = newExpression.Members.Select(m => MappingStrategyParser.Parse(m.Name)).ToArray();
                var keyProperties = _mapping.Fields.Where(f => propertyNames.Any(p => p.CaseInsensitiveEquals(f.Name))).ToArray();
                if (!keyProperties.Any())
                {
                    throw new ArgumentException($"{nameof(DapperMetadataBuilder<T>)}.{nameof(DapperMetadataBuilder<T>.HasKey)} 初始化对象表达式中至少要指定一个属性。");
                }
                if (keyProperties.Any(k => k.Ignore))
                {
                    throw new ArgumentException($"{nameof(DapperMetadataBuilder<T>)}.{nameof(DapperMetadataBuilder<T>.HasKey)} 不能指定已忽略的属性作为 Key 。");
                }
                keyProperties.ForEach(f => f.IsKey = true);
                return this;
            }

            throw new ArgumentException($"{nameof(HasKey)} 方法使用了不支持的表达式作为 {nameof(expression)} 参数。");
        }

        /// <summary>
        /// 指定实体使用特定的数据源（数据库）。
        /// </summary>
        /// <param name="connnectionName">数据库连接字符串名称。</param>
        /// <returns></returns>
        public DapperMetadataBuilder<T> DataSource<TDatabaseProvider>(String connnectionName)
            where TDatabaseProvider : IDatabaseProvider
        {
            this.DataSource(connnectionName, connnectionName);
            return this;
        }

        /// <summary>
        /// 指定实体使用特定的数据源（数据库）。
        /// </summary>
        /// <param name="connectionName">数据库的连接字符串名称（配置中的连接名称）。</param>
        /// <returns></returns>
        public DapperMetadataBuilder<T> DataSource(String connectionName)
        {
            return this.DataSource(connectionName, connectionName);
        }

        /// <summary>
        /// 指定实体使用特定的数据源（数据库）。
        /// </summary>
        /// <param name="readingConnection">读库的连接字符串名称（配置中的连接名称）。</param>
        /// <param name="writingConnection">写库的连接字符串名称（配置中的连接名称）。</param>
        /// <returns></returns>
        public DapperMetadataBuilder<T> DataSource(String writingConnection, String readingConnection)
        {
            _mapping.DbReadingConnectionName = readingConnection;
            _mapping.DbWritingConnectionName = writingConnection;
            return this;
        }

        /// <summary>
        /// 忽略实体属性。
        /// </summary>
        /// <param name="propertyExpression">表示实体属性的表达式（示例：a=>a.P ）。</param>
        /// <returns></returns>
        public DapperMetadataBuilder<T> Ignore(Expression<Func<T, Object>> propertyExpression)
        {
            Guard.ArgumentNotNull(propertyExpression, nameof(propertyExpression));

            Expression visited = propertyExpression.Body;
            UnaryExpression unary = visited as UnaryExpression;
            if (unary != null)
            {
                visited = unary.Operand;
            }

            MemberExpression mex = visited as MemberExpression;
            if (mex != null)
            {
                string name = mex.Member.Name;
                var field = this._mapping.Fields.FirstOrDefault(k => k.Name.CaseInsensitiveEquals(name));
                if (field != null)
                {
                    if (field.IsKey)
                    {
                        throw new ArgumentException($"不能使用 {nameof(DapperMetadataBuilder<T>)}.{nameof(Ignore)} 方法忽略已经被指定为 Key 的属性。");
                    }
                    field.Ignore = true;
                }
            }
            else
            {
                throw new ArgumentException($@"{nameof(DapperMetadataBuilder<T>)}.{nameof(Ignore)} 
                                                                        方法只接受单个属性表达式，例如 a=>a.Property。");
            }
            return this;
        }

        internal DapperMetadata Build()
        {
            if (!_mapping.Fields.Any(f => f.IsKey))
            {
                throw new InvalidOperationException($"没有为 Dapper 实体类型 {typeof(T).Name} 指定主键（请使用 {nameof(DapperMetadataBuilder<T>)}.{nameof(HasKey)} 方法指定）。");
            }
            return _mapping;
        }
    }
}
