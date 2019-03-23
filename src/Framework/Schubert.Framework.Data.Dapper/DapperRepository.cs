using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Schubert.Framework.Data
{
    public class DapperRepository<T> : IRepository<T>
        where T : class
    {
        private DapperMetadata _metadata = null;
        private Lazy<ILogger> _logger = null;
        private DapperDataSource _dataSource;

        public DapperRepository(DapperContext dapperContext, ILoggerFactory loggerFactory = null)
        {
            Guard.ArgumentNotNull(dapperContext, nameof(dapperContext));
            Context = dapperContext;
            _metadata = dapperContext.Runtime.GetMetadata(typeof(T));
            this.EntityType = typeof(T);
            _logger = new Lazy<ILogger>(() => loggerFactory?.CreateLogger<DapperRepository<T>>() ?? (ILogger)NullLogger.Instance);
        }

        protected ILogger Logger
        {
            get { return _logger.Value; }
        }

        protected DapperDataSource DataSource
        {
            get { return _dataSource ?? (_dataSource = this.Context.Runtime.GetDataSource(typeof(T)));  }
        }

        /// <summary>
        /// 派生类中实现时表示获得读库的连接字符串。
        /// </summary>
        /// <returns></returns>
        protected virtual IDbConnection GetReadingConnection()
        {
            return Context.GetConnection(this.DataSource.ReadingConnectionName);
        }

        /// <summary>
        /// 派生类中实现时表示获得写库的连接字符串。
        /// </summary>
        /// <returns></returns>
        protected virtual IDbConnection GetWritingConnection()
        {
            return Context.GetConnection(this.DataSource.WritingConnectionName);
        }

        /// <summary>
        /// 获取当前持久化实体的类型。
        /// </summary>
        protected Type EntityType { get; }

        /// <summary>
        /// 获取当前仓储的 Dapper 上下文。
        /// </summary>
        protected DapperContext Context { get; }

        #region Sql Generation

        private DapperSqlSegment GenerateCountSql(QueryFilter filter)
        {
            var sql = new DapperSqlSegment();

            var whereSeg = Context.Runtime.SqlGenerator.GenerateFilter<T>(filter, sql.Parameters);
            sql.Sql = string.IsNullOrWhiteSpace(whereSeg) ? $"SELECT COUNT(*) FROM {this.Context.Runtime.DelimitIdentifier(typeof(T), this._metadata.TableName)}"
                : $"SELECT COUNT(*) FROM {this.Context.Runtime.DelimitIdentifier(typeof(T), this._metadata.TableName)} WHERE {whereSeg}";
            return sql;
        }

        private DapperSqlSegment GenerateQuerySql(QueryFilter filter)
        {
            var sql = new DapperSqlSegment();

            var segments = Context.Runtime.GetCrudSegments(this.EntityType);
            sql.Sql = segments.SelectSql;
            var whereSeg = Context.Runtime.SqlGenerator.GenerateFilter<T>(filter, sql.Parameters);
            if (string.IsNullOrWhiteSpace(whereSeg)) return sql;
            sql.Sql = $"{sql.Sql} WHERE {whereSeg}";
            return sql;
        }

        private DapperSqlSegment GenerateQueryInSql(string field, IEnumerable<Object> values)
        {
            var sql = new DapperSqlSegment();

            var segments = Context.Runtime.GetCrudSegments(this.EntityType);
            sql.Sql = segments.SelectSql;
            var inSeg = Context.Runtime.SqlGenerator.GenerateInClause<T>(field, values, sql.Parameters);
            if (string.IsNullOrWhiteSpace(inSeg)) return sql;
            sql.Sql = $"{sql.Sql} WHERE {inSeg}";
            return sql;
        }

        private DapperSqlSegment GenerateUpdateSql(QueryFilter filter, IEnumerable<KeyValuePair<String, Object>> fieldsToUpdate)
        {
            var segment = new DapperSqlSegment();

            if (!fieldsToUpdate.Any())
            {
                throw new ArgumentException($"{typeof(IRepository<>).FullName}.{nameof(Update)} 方法的 {nameof(fieldsToUpdate)} 参数不可为 null 或空字典，如更新整个实体，考虑使用不包含 {nameof(fieldsToUpdate)} 参数的 {nameof(Update)} 重载方法。");
            }
            string tableName = Context.Runtime.GetMetadata(this.EntityType).TableName;
            string sets = this.Context.Runtime.SqlGenerator.GenerateSetSegments<T>(fieldsToUpdate, segment.Parameters);
            string where = this.Context.Runtime.SqlGenerator.GenerateFilter<T>(filter, segment.Parameters);
            if (string.IsNullOrWhiteSpace(where))
            {
                throw new NotSupportedException("不支持没有where条件的UPDATE语句");
            }
            segment.Sql = $"UPDATE {this.Context.Runtime.DelimitIdentifier(typeof(T), tableName)} SET {sets} WHERE {where}";
            return segment;
        }


        private DapperSqlSegment GenerateUpdateSql(T entity, IEnumerable<KeyValuePair<String, Object>> fieldsToUpdate)
        {
            var segment = new DapperSqlSegment();

            if (!fieldsToUpdate.Any())
            {
                throw new ArgumentException($"{typeof(IRepository<>).FullName}.{nameof(Update)} 方法的 {nameof(fieldsToUpdate)} 参数不可为 null 或空字典，如更新整个实体，考虑使用不包含 {nameof(fieldsToUpdate)} 参数的 {nameof(Update)} 重载方法。");
            }
            string tableName = Context.Runtime.GetMetadata(this.EntityType).TableName;
            string sets = this.Context.Runtime.SqlGenerator.GenerateSetSegments<T>(fieldsToUpdate, segment.Parameters);
            string where = this.Context.Runtime.SqlGenerator.GeneratePrimaryKeysWhereClause(entity, segment.Parameters);
            if (string.IsNullOrWhiteSpace(where))
            {
                throw new NotSupportedException("不支持没有where条件的UPDATE语句");
            }
            segment.Sql = $"UPDATE {this.Context.Runtime.DelimitIdentifier(typeof(T), tableName)} SET {sets} WHERE {where}";

            return segment;
        }

        private DapperSqlSegment GeneratePaginationSql(int pageIndex, int pageSize, QueryFilter filter, SortOptions options)
        {
            DapperSqlSegment segment = new DapperSqlSegment();

            options = options ?? new SortOptions(this._metadata.Fields.First(f => f.IsKey).Name);

            var segments = Context.Runtime.GetCrudSegments(this.EntityType);
            string select = segments.SelectSql;

            string where = filter != null ? Context.Runtime.SqlGenerator.GenerateFilter<T>(filter, segment.Parameters) : null;

            string order = Context.Runtime.SqlGenerator.GenerateOrderBy<T>(options);

            segment.Sql = Context.Runtime.GetDataSource(typeof(T)).DatabaseProvider.BuildPaginationTSql(pageIndex, pageSize, select, order, where);

            return segment;
        }

        #endregion

        public T QueryFirstOrDefault(QueryFilter filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            var sql = GenerateQuerySql(filter);

            var connection = this.GetReadingConnection();
            return connection.QueryFirstOrDefault<T>(sql.Sql, sql.Parameters);
        }

        public Task<T> QueryFirstOrDefaultAsync(QueryFilter filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            var sql = GenerateQuerySql(filter);

            var connection = this.GetReadingConnection();
            return connection.QueryFirstOrDefaultAsync<T>(sql.Sql, sql.Parameters);
        }

        public int Count()
        {
            string tableIdentifier = this.Context.Runtime.DelimitIdentifier(typeof(T), this._metadata.TableName);
            string sql = $"SELECT COUNT(*) FROM {tableIdentifier}";
            var connection = this.GetReadingConnection();
            return connection.ExecuteScalar<int>(sql);
        }

        public Task<int> CountAsync()
        {
            string tableIdentifier = this.Context.Runtime.DelimitIdentifier(typeof(T), this._metadata.TableName);
            string sql = $"SELECT COUNT(*) FROM {tableIdentifier}";
            var connection = this.GetReadingConnection();
            return connection.ExecuteScalarAsync<int>(sql);
        }

        public int Count(QueryFilter filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            var sql = GenerateCountSql(filter);
            var connection = this.GetReadingConnection();
            return connection.ExecuteScalar<int>(sql.Sql, sql.Parameters);
        }

        public Task<int> CountAsync(QueryFilter filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            var sql = GenerateCountSql(filter);
            var connection = this.GetReadingConnection();
            return connection.ExecuteScalarAsync<int>(sql.Sql, sql.Parameters);
        }

        public IEnumerable<T> QueryAll()
        {
            var segments = Context.Runtime.GetCrudSegments(this.EntityType);
            string sql = segments.SelectSql;

            var connection = this.GetReadingConnection();
            return connection.Query<T>(sql);
        }

        public Task<IEnumerable<T>> QueryAllAsync()
        {
            var segments = Context.Runtime.GetCrudSegments(this.EntityType);
            string sql = segments.SelectSql;

            var connection = this.GetReadingConnection();
            return connection.QueryAsync<T>(sql);
        }

        public IEnumerable<T> QueryPage(int pageIndex, int pageSize, QueryFilter filter = null, SortOptions options = null)
        {
            var sql = GeneratePaginationSql(pageIndex, pageSize, filter, options);

            var connection = this.GetReadingConnection();
            return connection.Query<T>(sql.Sql, sql.Parameters);
        }

        public Task<IEnumerable<T>> QueryPageAsync(int pageIndex, int pageSize, QueryFilter filter = null, SortOptions options = null)
        {
            var sql = GeneratePaginationSql(pageIndex, pageSize, filter, options);

            var connection = this.GetReadingConnection();
            return connection.QueryAsync<T>(sql.Sql, sql.Parameters);
        }


        public IEnumerable<T> Query<TField>(string fieldName, IEnumerable<TField> fieldValues)
        {
            Guard.ArgumentNullOrWhiteSpaceString(fieldName, nameof(fieldName));
            if (fieldValues.IsNullOrEmpty())
            {
                return Enumerable.Empty<T>();
            }
            if (fieldValues.Count() == 1)
            {
                SingleQueryFilter filter = new SingleQueryFilter();
                filter.AddEqual(fieldName, fieldValues.First());
                return this.Query(filter);
            }
            else
            {
                var sql = GenerateQueryInSql(fieldName, fieldValues.Cast<Object>());

                var connection = this.GetReadingConnection();
                return connection.Query<T>(sql.Sql, sql.Parameters);
            }
        }

        public Task<IEnumerable<T>> QueryAsync<TField>(string fieldName, IEnumerable<TField> fieldValues)
        {
            Guard.ArgumentNullOrWhiteSpaceString(fieldName, nameof(fieldName));
            if (fieldValues.IsNullOrEmpty())
            {
                return Task.FromResult(Enumerable.Empty<T>());
            }
            if (fieldValues.Count() == 1)
            {
                SingleQueryFilter filter = new SingleQueryFilter();
                filter.AddEqual(fieldName, fieldValues.First());
                return this.QueryAsync(filter);
            }
            else
            {
                var sql = GenerateQueryInSql(fieldName, fieldValues.Cast<Object>());

                var connection = this.GetReadingConnection();
                return connection.QueryAsync<T>(sql.Sql, sql.Parameters);
            }
        }

        public IEnumerable<T> Query(QueryFilter filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            var sql = GenerateQuerySql(filter);

            var connection = this.GetReadingConnection();
            return connection.Query<T>(sql.Sql, sql.Parameters);
        }

        public Task<IEnumerable<T>> QueryAsync(QueryFilter filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            var sql = GenerateQuerySql(filter);

            var connection = this.GetReadingConnection();
            return connection.QueryAsync<T>(sql.Sql, sql.Parameters);
        }


        public int Delete(T entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            var segments = Context.Runtime.GetCrudSegments(this.EntityType);

            var sql = segments.DeleteSql;

            var connection = this.GetWritingConnection();
            return connection.Execute(sql, entity);
        }

        public Task<int> DeleteAsync(T entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            var segments = Context.Runtime.GetCrudSegments(this.EntityType);

            string sql = segments.DeleteSql;

            var connection = this.GetWritingConnection();
            return connection.ExecuteAsync(sql, entity);
        }

        private static class PropertySetter
        {
            private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object>> cache
                = new ConcurrentDictionary<PropertyInfo, Action<object, object>>();

            private static readonly MethodInfo changer = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) });
            public static void SetProperty<E>(PropertyInfo property, E target, object value) where E : class
            {
                var setter = cache.GetOrAdd(property, prop =>
                {
                    var p1 = Expression.Parameter(typeof(object));
                    var p2 = Expression.Parameter(typeof(object));
                    var call = Expression.Call(Expression.Convert(p1, typeof(E)), property.SetMethod,
                        Expression.Convert(Expression.Call(null, changer, p2, Expression.Constant(prop.PropertyType)),
                        prop.PropertyType));
                    var lambda = Expression.Lambda<Action<object, object>>(call, p1, p2);
                    return lambda.Compile();
                });
                setter(target, value);
            }
        }



        private DapperFieldMetadata GetAutoGenerationField()
        {
            var supportTypes = new[] { typeof(int), typeof(uint), typeof(long), typeof(ulong) };
            var matedata = this.Context.Runtime.GetMetadata(this.EntityType);
            var fields = matedata.Fields
                .Where(f => f.IsKey && f.AutoGeneration && supportTypes.Contains(f.Field.PropertyType)).ToArray();
            if (Context.Runtime.GetDataSource(typeof(T)).DatabaseProvider.GetLastedInsertIdSupported && fields?.Length == 1)
            {
                return fields.First();
            }
            return null;
        }



        public int Insert(T entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));
            var segments = Context.Runtime.GetCrudSegments(this.EntityType);

            var sql = segments.InsertSql;
            var field = GetAutoGenerationField();

            var connection = this.GetWritingConnection();
            if (field != null)
            {
                using (var scope = new DbTransactionScope())
                {
                    var mergedSql = string.Concat(sql, ";", System.Environment.NewLine, Context.Runtime.GetDataSource(typeof(T)).DatabaseProvider.GetLastedInsertId());
                    var id = connection.ExecuteScalar(mergedSql, entity);

                    scope.Complete();

                    PropertySetter.SetProperty(field.Field, entity, id);
                    return 1;
                }
            }
            var count = connection.Execute(sql, entity);
            return count;
        }

        public async Task<int> InsertAsync(T entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));
            var segments = Context.Runtime.GetCrudSegments(this.EntityType);
            var sql = segments.InsertSql;
            var field = GetAutoGenerationField();

            var connection = this.GetWritingConnection();

            if (field != null)
            {
                using (var scope = new DbTransactionScope())
                {
                    var mergedSql = string.Concat(sql, ";", System.Environment.NewLine, Context.Runtime.GetDataSource(typeof(T)).DatabaseProvider.GetLastedInsertId());
                    var id = await connection.ExecuteScalarAsync(mergedSql, entity);

                    scope.Complete();

                    PropertySetter.SetProperty(field.Field, entity, id);
                    return 1;
                }
            }

            var count = await connection.ExecuteAsync(sql, entity);
            return count;
        }


        public Task<int> UpdateAsync(T entity, IEnumerable<KeyValuePair<String, Object>> fieldsToUpdate)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));
            Guard.ArgumentNotNull(fieldsToUpdate, nameof(fieldsToUpdate));

            var sql = GenerateUpdateSql(entity, fieldsToUpdate);
            var connection = this.GetWritingConnection();
            return connection.ExecuteAsync(sql.Sql, sql.Parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldsToUpdate"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Update(T entity, IEnumerable<KeyValuePair<string, object>> fieldsToUpdate)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));
            Guard.ArgumentNotNull(fieldsToUpdate, nameof(fieldsToUpdate));

            var sql = GenerateUpdateSql(entity, fieldsToUpdate);
            var connection = this.GetWritingConnection();
            return connection.Execute(sql.Sql, sql.Parameters);
        }

        public int Update(QueryFilter filter, IEnumerable<KeyValuePair<String, Object>> fieldsToUpdate)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));
            Guard.ArgumentNotNull(fieldsToUpdate, nameof(fieldsToUpdate));

            var sql = GenerateUpdateSql(filter, fieldsToUpdate);
            var connection = this.GetWritingConnection();
            return connection.Execute(sql.Sql, sql.Parameters);
        }

        public Task<int> UpdateAsync(QueryFilter filter, IEnumerable<KeyValuePair<String, Object>> fieldsToUpdate)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));
            Guard.ArgumentNotNull(fieldsToUpdate, nameof(fieldsToUpdate));

            var sql = GenerateUpdateSql(filter, fieldsToUpdate);
            var connection = this.GetWritingConnection();
            return connection.ExecuteAsync(sql.Sql, sql.Parameters);
        }


        public int Update(T entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            var segments = Context.Runtime.GetCrudSegments(this.EntityType);

            string sql = segments.UpdateSql;

            var connection = this.GetWritingConnection();
            return connection.Execute(sql, entity);
        }

        public Task<int> UpdateAsync(T entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            var segments = Context.Runtime.GetCrudSegments(this.EntityType);

            string sql = segments.UpdateSql;

            var connection = this.GetWritingConnection();
            return connection.ExecuteAsync(sql, entity);
        }

        public int Delete(QueryFilter filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            DynamicParameters parameters = new DynamicParameters();
            string where = this.Context.Runtime.SqlGenerator.GenerateFilter<T>(filter, parameters);
            string sql = $"DELETE FROM {Context.Runtime.DelimitIdentifier(typeof(T), this._metadata.TableName)} WHERE {where}";

            return this.GetWritingConnection().Execute(sql, parameters);
        }

        public Task<int> DeleteAsync(QueryFilter filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            DynamicParameters parameters = new DynamicParameters();
            string where = this.Context.Runtime.SqlGenerator.GenerateFilter<T>(filter, parameters);
            string sql = $"DELETE FROM {Context.Runtime.DelimitIdentifier(typeof(T), this._metadata.TableName)} WHERE {where}";

            return this.GetWritingConnection().ExecuteAsync(sql, parameters);
        }

        public int Insert(IEnumerable<T> entities)
        {
            if (entities.IsNullOrEmpty())
            {
                return 0;
            }
            if (!this.Context.Runtime.GetDataSource(typeof(T)).DatabaseProvider.BuildBatchInsertSqlSupported)
            {
                var segments = Context.Runtime.GetCrudSegments(this.EntityType);
                var sql = segments.InsertSql;
                var connection = this.GetWritingConnection();
                return connection.Execute(sql, entities);
            }
            else
            {
                BuildBatchInsertSql(entities, out var parameters, out var sql);
                var connection = this.GetWritingConnection();
                return connection.Execute(sql, parameters);
            }
        }

        private void BuildBatchInsertSql(IEnumerable<T> entities, out DynamicParameters parameters, out string sql)
        {
            var metadata = this.Context.Runtime.GetMetadata(typeof(T));

            parameters = new DynamicParameters();
            var fields = metadata.Fields.Where(f => f.Ignore == false && f.AutoGeneration == false).ToArray();
            List<Object[]> values = new List<object[]>(entities.Count());
            foreach (var e in entities)
            {
                Object[] dbValues = new Object[fields.Length];
                var index = 0;
                foreach (var f in fields)
                {
                    dbValues[index] = f.Field.GetValue(e);
                    index++;
                }
                values.Add(dbValues);
            }

            var list = new List<KeyValuePair<string, object>>();

            sql = this.Context.Runtime.GetDataSource(typeof(T)).DatabaseProvider.BuildBatchInsertSql(metadata.TableName, fields.Select(f => f.Name).ToArray(), values,
                (n, v) =>
                {
                    list.Add(new KeyValuePair<string, object>(n, v));
                });
            foreach (var item in list)
            {
                parameters.Add(item.Key, item.Value);
            }
        }

        public Task<int> InsertAsync(IEnumerable<T> entities)
        {
            if (entities.IsNullOrEmpty())
            {
                return Task.FromResult(0);
            }
            if (!this.Context.Runtime.GetDataSource(typeof(T)).DatabaseProvider.BuildBatchInsertSqlSupported)
            {
                var segments = Context.Runtime.GetCrudSegments(this.EntityType);
                var sql = segments.InsertSql;
                var connection = this.GetWritingConnection();
                return connection.ExecuteAsync(sql, entities);
            }
            else
            {
                BuildBatchInsertSql(entities, out var parameters, out var sql);
                var connection = this.GetWritingConnection();
                return connection.ExecuteAsync(sql, parameters);
            }
        }
    }
}
