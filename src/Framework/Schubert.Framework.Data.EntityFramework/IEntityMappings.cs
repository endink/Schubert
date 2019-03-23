using Microsoft.EntityFrameworkCore;

namespace Schubert.Framework.Data
{
    public interface IEntityMappings : ISingletonDependency
    {
        void ApplyMapping(ModelBuilder builder, IDbProvider dbProvider);
    }
}
