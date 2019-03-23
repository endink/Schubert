using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Schubert.Framework.Data
{
    public abstract class EntityMapping<TEntity> : IEntityMappings
        where TEntity : class
    {
        public void ApplyMapping(ModelBuilder builder, IDbProvider dbProvider)
        {
            this.DbProvider = dbProvider;

            EntityTypeBuilder<TEntity> typeBuilder = builder.Entity<TEntity>();
            typeBuilder.ToTable(this.StoreTableName);
            this.OnApply(typeBuilder);
        }

        protected abstract string StoreTableName { get; }

        protected IDbProvider DbProvider { get; private set; }

        protected abstract void OnApply(EntityTypeBuilder<TEntity> builder);
    }
}