using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public interface IRepository<T> where T : class
    {
        int Delete(T entity);
        Task<int> DeleteAsync(T entity);
        int Delete(QueryFilter filter);
        Task<int> DeleteAsync(QueryFilter filter);

        int Insert(T entity);

        Task<int> InsertAsync(T entity);

        int Insert(IEnumerable<T> entities);
        Task<int> InsertAsync(IEnumerable<T> entities);

        int Update(T entity);
        Task<int> UpdateAsync(T entity);
        Task<int> UpdateAsync(T entity, IEnumerable<KeyValuePair<String, Object>> fieldsToUpdate);
        int Update(T entity, IEnumerable<KeyValuePair<String, Object>> fieldsToUpdate);
        int Update(QueryFilter filter, IEnumerable<KeyValuePair<String, Object>> fieldsToUpdate);
        Task<int> UpdateAsync(QueryFilter filter, IEnumerable<KeyValuePair<String, Object>> fieldsToUpdate);

        int Count();
        Task<int> CountAsync();
        int Count(QueryFilter filter);
        Task<int> CountAsync(QueryFilter filter);

        IEnumerable<T> QueryPage(int pageIndex, int pageSize, QueryFilter filter = null, SortOptions options = null);
        Task<IEnumerable<T>> QueryPageAsync(int pageIndex, int pageSize, QueryFilter filter = null, SortOptions options = null);
        IEnumerable<T> Query<TField>(string fieldName, IEnumerable<TField> fieldValues);
        Task<IEnumerable<T>> QueryAsync<TField>(string fieldName, IEnumerable<TField> fieldValues);
        IEnumerable<T> Query(QueryFilter filter);
        IEnumerable<T> QueryAll();
        Task<IEnumerable<T>> QueryAllAsync();
        Task<IEnumerable<T>> QueryAsync(QueryFilter filter);

        T QueryFirstOrDefault(QueryFilter filter);
        Task<T> QueryFirstOrDefaultAsync(QueryFilter filter);

    }
}