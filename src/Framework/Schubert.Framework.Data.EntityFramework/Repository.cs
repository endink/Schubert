using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public class Repository<T, TDbContext> : IDisposable, IRepository<T, TDbContext>
        where T : class
        where TDbContext : DbContext
    {
        private DbContext _context;
        private DbSet<T> _entities;
        private IOptions<DbOptions> _dbOptions;
        private SqlHelper _sqlHelper;

        protected DbContext Context { get { return _context; } }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="context">Object context</param>
        /// <param name="dbOptions"></param>
        public Repository(TDbContext context, IOptions<DbOptions> dbOptions)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(dbOptions, nameof(dbOptions));

            this._context = context;
            this._dbOptions = dbOptions;
            //CloudStorageAccount.DevelopmentStorageAccount
        }

        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IQueryable<T> Table => this.Entities;
        /// <summary>
        /// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        public virtual IQueryable<T> TableNoTracking => this.Entities.AsNoTracking();
        /// <summary>
        /// Entities
        /// </summary>
        protected virtual DbSet<T> Entities => (_entities ?? (_entities = _context?.Set<T>()));
        /// <summary>
        /// helper for use t-sql
        /// </summary>
        public SqlHelper SqlHelper => (_sqlHelper ?? (_sqlHelper = new SqlHelper(_context, _dbOptions)));

        public T QueryFirstOrDefault(IDictionary<string, object> propertyEqualsFilter)
        {
            Expression<Func<T, bool>> predicate = BuildQueryPredicate(propertyEqualsFilter);
            return this.Entities.FirstOrDefault(predicate);
        }


        public async Task<T> QueryFirstOrDefaultAsync(IDictionary<string, object> propertyEqualsFilter)
        {
            Expression<Func<T, bool>> predicate = BuildQueryPredicate(propertyEqualsFilter);
            T result = await this.Entities.FirstOrDefaultAsync(predicate);
            return result;
        }

        private static Expression<Func<T, bool>> BuildQueryPredicate(IDictionary<string, object> propertyEqualsFilter)
        {
            if (propertyEqualsFilter.IsNullOrEmpty())
            {
                throw new ArgumentException("属性筛选器字典不能为空。", nameof(propertyEqualsFilter));
            }
            var parameter = Expression.Parameter(typeof(T), "o");
            BinaryExpression binaryExpression = null;
            foreach (var propertyValue in propertyEqualsFilter)
            {
                PropertyInfo propertyInfo = typeof(T).GetTypeInfo().DeclaredProperties
                    .FirstOrDefault(p => propertyValue.Key.Equals(p.Name, StringComparison.Ordinal) && p.CanRead && !p.GetMethod.IsStatic);

                if (propertyInfo == null)
                {
                    throw new ArgumentException($"属性筛选字典中包含了 {propertyValue.Key} 键， 但是类型 {typeof(T).Name} 中找不到名为 {propertyValue.Key} 的属性。", nameof(propertyEqualsFilter));
                }
                var propertyExpression = Expression.MakeMemberAccess(parameter, propertyInfo);
                var valueExpression = propertyValue.Value == null ? Expression.Constant(null) : Expression.Constant(propertyValue.Value, propertyInfo.PropertyType);
                var equalExpression = Expression.Equal(propertyExpression, valueExpression);
                if (binaryExpression == null)
                {
                    binaryExpression = equalExpression;
                }
                else
                {
                    binaryExpression = Expression.And(binaryExpression, equalExpression);
                }
            }
            Expression<Func<T, bool>> predicate = (Expression<Func<T, bool>>)Expression.Lambda(binaryExpression, parameter);
            return predicate;
        }


        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Insert(T entity)
        {
            ValidateNullEntity(entity);
            this.Entities.Add(entity);
        }

        private static void ValidateNullEntity(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="updateConcurrencyStamp">更新并发列。</param>
        public virtual void Update(T entity, Action<T> updateConcurrencyStamp = null)
        {
            ValidateNullEntity(entity);
            _context.Attach(entity);
            updateConcurrencyStamp?.Invoke(entity);
            _context.Update(entity);
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Delete(T entity)
        {
            ValidateNullEntity(entity);

            this.Entities.Remove(entity);
        }

        public int CommitChanges()
        {
            try
            {
                int result = _context.SaveChanges();
                return result;
            }
            catch (DbUpdateException ex)
            {
                HandleException(ex);
                throw;
            }
        }

        public async Task<int> CommitChangesAsync()
        {
            try
            {
                int result = await _context.SaveChangesAsync();
                return result;
            }
            catch (DbUpdateException ex)
            {
                HandleException(ex);
                throw;
            }
        }

        private void HandleException(DbUpdateException ex)
        {
            var provider = _dbOptions.Value.GetDbProvider(_context.GetType());
            if (provider.HasUniqueConstraintViolation(ex))
            {
                throw new UniqueConstraintViolationException(ex.Message, ex.InnerException);
            }
        }

        public void Dispose()
        {
            _context = null;
            _entities = null;
        }

        public void ClearTracking()
        {
            _context.DetachEntities(e => typeof(T).Equals(e.Entity?.GetType()));
        }

        public void Update(T existedEntity, T updatedEntity)
        {
            Guard.ArgumentNotNull(existedEntity, nameof(existedEntity));
            Guard.ArgumentNotNull(updatedEntity, nameof(existedEntity));

            var entry = _context.Entry(existedEntity);
            entry.CurrentValues.SetValues(updatedEntity);
        }
    }

    public class Repository<T> : Repository<T, DbContext>
        where T : class
    {
        public Repository(DbContext dbContext, IOptions<DbOptions> dbOptions)
            : base(dbContext, dbOptions)
        {
            
        }
    }
}