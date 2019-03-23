using Microsoft.EntityFrameworkCore;
using System;

namespace Schubert.Framework.Data
{
    public interface IDatabaseInitializer
    {
        void CreateModel(ModelBuilder builder, DbContext context);
        void InitializeContext<T>(T context)
            where T : DbContext, INewDatabaseFlag;
    }
}
