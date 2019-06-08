using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Linq;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Schubert.Framework.Data.EntityFramework;

namespace Schubert.Framework.Data
{
    public class DefaultDbContext : DbContext, INewDatabaseFlag
    {
        public bool IsNew { get; set; }

        public DefaultDbContext(DbContextOptions options)
            :base(options)
        {
            //DatabaseInitializer.Default.InitializeContext(this);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.GetDbProvider()?.OnCreateModel(modelBuilder, this.GetDbOptions());
        }
    }
}
