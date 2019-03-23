using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// 根据指定的属性值过滤来获取实体。
        /// </summary>
        /// <param name="propertyEqualsFilter">用于查询过滤属性名和属性值的字典。</param>
        /// <returns>如果查询到实体，返回第一个实体对象；否则返回 null。</returns>
        T QueryFirstOrDefault(IDictionary<String, object> propertyEqualsFilter);

        /// <summary>
        /// 根据指定的属性值过滤异步获取实体。
        /// </summary>
        /// <param name="propertyEqualsFilter">用于查询过滤属性名和属性值的字典。</param>
        /// <returns>如果查询到实体，返回第一个实体对象；否则返回 null。</returns>
        Task<T> QueryFirstOrDefaultAsync(IDictionary<String, object> propertyEqualsFilter);

        /// <summary>
        /// 从上下文中移除所有被追踪的实体。
        /// </summary>
        void ClearTracking();

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Insert(T entity);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="updateConcurrencyStamp">更新行并发标识的操作。</param>
        /// <param name="entity">Entity</param>
        void Update(T entity, Action<T> updateConcurrencyStamp = null);

        /// <summary>
        /// 对已经存在于上下文中的实体进行更新。
        /// </summary>
        /// <param name="existedEntity"></param>
        /// <param name="updatedEntity"></param>
        void Update(T existedEntity, T updatedEntity);

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Delete(T entity);

        int CommitChanges();

        Task<int> CommitChangesAsync();

        /// <summary>
        /// Gets a table
        /// </summary>
        IQueryable<T> Table { get; }

        /// <summary>
        /// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        IQueryable<T> TableNoTracking { get; }

        SqlHelper SqlHelper { get; }
    }

    public interface IRepository<T, TContext> : IRepository<T>
        where T : class
        where TContext: DbContext
    {
        
    }
}