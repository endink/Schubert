using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// SQL 生成器对象。
    /// </summary>
    public class DapperSqlGenerator
    {
        private DapperRuntime _runtime = null;
        public DapperSqlGenerator(DapperRuntime runtime)
        {
            Guard.ArgumentNotNull(runtime, nameof(runtime));

            _runtime = runtime;
        }

        /// <summary>
        /// 根据字段名和值生成 SET 子句（不包含 SET 字符）。
        /// </summary>
        /// <param name="fieldsAndValues">要生成 SET 子句的字段名和字段值。</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string GenerateSetSegments<TEntity>(IEnumerable<KeyValuePair<String, Object>> fieldsAndValues, DynamicParameters parameters)
        {
            Guard.ArgumentNotNull(fieldsAndValues, nameof(fieldsAndValues));

            List<String> setString = new List<String>();
            foreach (var kp in fieldsAndValues)
            {
                string parameterName = $"p{(parameters.ParameterNames.Count() + 1)}";
                parameters.Add($"{parameterName}", kp.Value);
                setString.Add($"{_runtime.DelimitIdentifier(typeof(TEntity), MappingStrategyParser.Parse(kp.Key))} = {_runtime.DelimitParameter(typeof(TEntity), parameterName)}");
            }
            return setString.ToArrayString(", ");
        }

        /// <summary>
        /// 生成实体主键的  Where 从句（不包含 Where 字符）。
        /// </summary>
        /// <param name="entity">实体类型</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public String GeneratePrimaryKeysWhereClause<TEntity>(TEntity entity, DynamicParameters parameters)
        {
            Guard.ArgumentNotNull(parameters, nameof(parameters));
            Guard.ArgumentNotNull(parameters, nameof(parameters));

            var keys = this._runtime.GetMetadata(typeof(TEntity)).Fields.Where(f => f.IsKey).ToArray();
            List<String> whereSet = new List<String>();
            foreach (var k in keys)
            {
                string parameterName = $"p{parameters.ParameterNames.Count() + 1}";
                whereSet.Add($"{_runtime.DelimitIdentifier(typeof(TEntity), MappingStrategyParser.Parse(k.Name))} = {_runtime.DelimitParameter(typeof(TEntity), parameterName)}");
                parameters.Add(parameterName, k.Field.GetValue(entity));
            }
            return whereSet.ToArrayString(" AND ");
        }

        /// <summary>
        /// 生成 Order By 子句。
        /// </summary>
        /// <param name="sortOptions">排序选项。</param>
        /// <returns></returns>
        public string GenerateOrderBy<TEntity>(SortOptions sortOptions)
        {
            Guard.ArgumentNotNull(sortOptions, nameof(sortOptions));

            string fields = sortOptions.GetFields().Select(f => _runtime.DelimitIdentifier(typeof(TEntity), MappingStrategyParser.Parse(f))).ToArrayString(", ");
            string sql = $" ORDER BY {fields}";
            if (sortOptions.SortOrder == SortOrder.Descending)
            {
                sql = String.Concat(sql, " desc");
            }
            return sql;
        }

        /// <summary>
        /// 生成 In 从句。
        /// </summary>
        /// <param name="fieldName">字段名称。</param>
        /// <param name="values">字段值的集合。</param>
        /// <param name="parameters">查询参数</param>
        /// <returns></returns>
        public string GenerateInClause<TEntity>(string fieldName, IEnumerable<Object> values, DynamicParameters parameters)
        {
            if (fieldName.IsNullOrWhiteSpace() || values.IsNullOrEmpty())
            {
                return null;
            }
            string parameterName = $"p{parameters.ParameterNames.Count() + 1}";
            parameters.Add(parameterName, values);
            return $"{_runtime.DelimitIdentifier(typeof(TEntity), MappingStrategyParser.Parse(fieldName))} IN {_runtime.DelimitParameter(typeof(TEntity), parameterName)}";
        }

        /// <summary>
        /// 生成查询过滤。
        /// </summary>
        /// <param name="queryFilter">查询过来器实例。</param>
        /// <param name="parameters">查询参数</param>
        /// <returns></returns>
        public String GenerateFilter<TEntity>(QueryFilter queryFilter, DynamicParameters parameters)
        {
            if (queryFilter != null)
            {
                SingleQueryFilter sqf = queryFilter as SingleQueryFilter;
                if (sqf != null)
                {
                    return this.GenerateSingleFilter<TEntity>(sqf, parameters);
                }

                CombinedQueryFilter cqf = queryFilter as CombinedQueryFilter;
                if (cqf != null)
                {
                    return GenerateCombinedFilter<TEntity>(cqf, parameters);
                }
            }
            return null;
        }

        private string GenerateCombinedFilter<TEntity>(CombinedQueryFilter queryFilter, DynamicParameters parameters)
        {
            string spliter = queryFilter.Clause == BooleanClause.Or ? " OR " : " AND ";
            string left = this.GenerateFilter<TEntity>(queryFilter.Filter1, parameters);
            string right = this.GenerateFilter<TEntity>(queryFilter.Filter2, parameters);

            if (!right.IsNullOrWhiteSpace())
            {
                if (left.IsNullOrWhiteSpace())
                {
                    return right;
                }
                else
                {
                    return $"({left}) {spliter} ({right})";
                }
            }
            else
            {
                return left.IfNullOrWhiteSpace(String.Empty);
            }
        }

        private string GenerateSingleFilter<TEntity>(SingleQueryFilter queryFilter, DynamicParameters parameters)
        {
            if (queryFilter.IsEmpty)
            {
                return String.Empty;
            }
            string spliter = queryFilter.Clause == BooleanClause.Or ? " OR " : " AND ";
            return queryFilter.Select(p => GeneratePredicateFilter<TEntity>(p, parameters)).ToArray().ToArrayString(spliter);
        }

        private string GeneratePredicateFilter<TEntity>(FieldPredicate predicate, DynamicParameters parameters)
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));


            if (predicate.OperationValue == null && predicate.Operation != BinaryOperation.Equal && predicate.Operation != BinaryOperation.NotEqual)
            {
                throw new InvalidOperationException($"FieldPredicate Error: 不可将空置用于 {predicate.Operation.ToString()} 类型的二元运算。");
            }


            string opString = "=";
            switch (predicate.Operation)
            {
                case BinaryOperation.Equal:
                    opString = "=";
                    break;
                case BinaryOperation.NotEqual:
                    opString = "<>";
                    break;
                case BinaryOperation.Greater:
                    opString = ">";
                    break;
                case BinaryOperation.GreaterOrEqual:
                    opString = ">=";
                    break;
                case BinaryOperation.Less:
                    opString = "<";
                    break;
                case BinaryOperation.LessOrEqual:
                    opString = "<=";
                    break;
            }

            if (predicate.OperationValue == null)
            {
                if (predicate.Operation == BinaryOperation.Equal)
                {
                    return $"{_runtime.DelimitIdentifier(typeof(TEntity), predicate.FieldName)} IS NULL";
                }
                if (predicate.Operation == BinaryOperation.NotEqual)
                {
                    return $"{_runtime.DelimitIdentifier(typeof(TEntity), predicate.FieldName)} IS NOT NULL";
                }
            }

            string p = $"p{parameters.ParameterNames.Count() + 1}";
            parameters.Add(p, predicate.OperationValue);
            return $"{_runtime.DelimitIdentifier(typeof(TEntity), predicate.FieldName)} {opString} {_runtime.DelimitParameter(typeof(TEntity), p)}";
        }
    }
}
