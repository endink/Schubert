using Schubert.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public static class IEntityRepositoryExtensions
    {
        public static T QueryFirstOrDefault<T>(this IRepository<T> repository, IDictionary<Expression<Func<T, Object>>, Object> idValues)
                where T : class
        {
            Dictionary<String, Object> propertyValues = BuildPropertyValues(idValues);
            return repository.QueryFirstOrDefault(propertyValues);
        }

        public static async Task<T> QueryFirstOrDefaultAsync<T>(this IRepository<T> repository, IDictionary<Expression<Func<T, Object>>, Object> idValues)
                where T : class
        {
            Dictionary<string, object> propertyValues = BuildPropertyValues(idValues);
            T result = await repository.QueryFirstOrDefaultAsync(propertyValues);
            return result;
        }

        private static Dictionary<string, object> BuildPropertyValues<T>(IDictionary<Expression<Func<T, object>>, object> idValues) where T : class
        {
            Dictionary<String, Object> propertyValues = new Dictionary<string, object>();
            foreach (var keyValue in idValues)
            {
                string propertyName = keyValue.Key.GetMemberName();
                object value = keyValue.Value;
                propertyValues.Add(propertyName, value);
            }

            return propertyValues;
        }

        /// <summary>
        /// 根据属性值模版过滤来获取实体。
        /// </summary>
        /// <param name="propertyTemplate">用于查询过滤属性名和属性值的表达式。例如 ()=> new T { Property1 = XX, Property2 = XX }</param>
        /// <param name="repository"></param>
        /// <returns>如果查询到实体，返回第一个实体对象；否则返回 null。</returns>
        public static async Task<T> QueryFirstOrDefaultAsync<T>(this IRepository<T> repository, Expression<Func<T>> propertyTemplate)
                where T : class
        {
            IDictionary<String, Object> propertyValues = ExpressionHelper.GetMembersAndValues(propertyTemplate);
            T result = await repository.QueryFirstOrDefaultAsync(propertyValues);
            return result;
        }



        /// <summary>
        /// 根据属性值模版过滤来获取实体。
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="propertyTemplate">用于查询过滤属性名和属性值的表达式。例如 ()=> new T { Property1 = XX, Property2 = XX }</param>
        /// <returns>如果查询到实体，返回第一个实体对象；否则返回 null。</returns>
        public static T QueryFirstOrDefault<T>(this IRepository<T> repository, Expression<Func<T>> propertyTemplate)
                where T : class
        {
            IDictionary<String, Object> propertyValues = ExpressionHelper.GetMembersAndValues(propertyTemplate);
            return repository.QueryFirstOrDefault(propertyValues);
        }

        /// <summary>
        /// 返回具有指定 Id 值的实体。
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="idValue">实体 Id 。</param>
        /// <param name="idPropertyName">实体的 Id 属性名称（默认为 Id）。</param>
        /// <returns></returns>
        public static T QueryById<T>(this IRepository<T> repository, object idValue, string idPropertyName = "Id")
                where T : class
        {
            Dictionary<String, Object> propertyValues = new Dictionary<string, object>() { { idPropertyName, idValue } };
            return repository.QueryFirstOrDefault(propertyValues);
        }

        /// <summary>
        /// 返回具有指定 Id 值的实体。
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="idValue">实体 Id 。</param>
        /// <param name="idPropertyName">实体的 Id 属性名称（默认为 Id）。</param>
        /// <returns></returns>
        public static async Task<T> QueryByIdAsync<T>(this IRepository<T> repository, object idValue, string idPropertyName = "Id")
                where T : class
        {
            Dictionary<String, Object> propertyValues = new Dictionary<string, object>() { { idPropertyName, idValue } };
            T result = await repository.QueryFirstOrDefaultAsync(propertyValues);
            return result;
        }

    }
}