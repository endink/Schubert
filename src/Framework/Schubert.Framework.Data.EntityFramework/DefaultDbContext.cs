using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Linq;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public class DefaultDbContext : DbContext, INewDatabaseFlag
    {
        private IDatabaseInitializer _databaseInitializer = null;

        public bool IsNew { get; set; }

        public DefaultDbContext(DbContextOptions options, IDatabaseInitializer databaseInitializer)
            :base(options)
        {
            Guard.ArgumentNotNull(databaseInitializer, nameof(databaseInitializer));
            _databaseInitializer = databaseInitializer;
            _databaseInitializer.InitializeContext(this);
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            _databaseInitializer.CreateModel(modelBuilder, this);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
