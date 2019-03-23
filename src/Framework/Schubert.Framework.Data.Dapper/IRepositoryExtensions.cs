using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public static class IRepositoryExtensions
    {
        public static int Count<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
            where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);
            return repository.Count(filter);
        }

        public static Task<int> CountAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
            where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);
            return repository.CountAsync(filter);
        }

        public static int Delete<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
            where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);
            return repository.Delete(filter);
        }

        public static Task<int> DeleteAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
            where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);
            return repository.DeleteAsync(filter);
        }

        public static IEnumerable<T> Query<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
            where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);
            return repository.Query(filter);
        }

        public static Task<IEnumerable<T>> QueryAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
             where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);
            return repository.QueryAsync(filter);
        }

        public static IEnumerable<T> QueryPage<T>(this IRepository<T> repository, int pageIndex, int pageSize, Expression<Func<T, bool>> predicate, SortOptions options = null)
            where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);

            return repository.QueryPage(pageIndex, pageSize, filter, options);
        }

        public static Task<IEnumerable<T>> QueryPageAsync<T>(this IRepository<T> repository, int pageIndex, int pageSize, Expression<Func<T, bool>> predicate, SortOptions options = null)
            where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);

            return repository.QueryPageAsync(pageIndex, pageSize, filter, options);
        }

        public static T QueryFirstOrDefault<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
            where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);
            return repository.QueryFirstOrDefault(filter);
        }
        public static Task<T> QueryFirstOrDefaultAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
             where T : class
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var filter = LamdaQueryParser.Where(predicate);
            return repository.QueryFirstOrDefaultAsync(filter);
        }
    }
}
