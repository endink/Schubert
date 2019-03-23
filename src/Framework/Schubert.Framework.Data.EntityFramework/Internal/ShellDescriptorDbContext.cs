using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schubert.Framework.Data.EntityFramework;
using Schubert.Framework.Data.Mappings;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Schubert.Framework.Data
{
    internal class ShellDescriptorDbContext : DbContext, INewDatabaseFlag
    {
        private static String _id = Guid.NewGuid().ToString();

        private static bool _created = false;
        private static readonly object CreateSync = new object();
        private DbOptions _options;
        private ILoggerFactory _loggerFactory;

        public bool IsNew { get; set; }

        public ShellDescriptorDbContext(IOptions<DbOptions> options, ILoggerFactory loggerFactory = null)
        {
            Guard.ArgumentNotNull(options, nameof(options));

            _loggerFactory = loggerFactory;
            _options = options.Value;
            if (!_created)
            {
                lock(CreateSync)
                {
                    if (!_created)
                    {
                        if (_options.CreateDatabaseIfNotExisting)
                        {
                            this.IsNew = this.Database.EnsureCreated();
                        }
                        _created = true;
                    }
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _options.GetDbProvider(typeof(ShellDescriptorDbContext)).OnBuildContext(this.GetType(), optionsBuilder, _options);
            base.OnConfiguring(optionsBuilder);
            if (_loggerFactory != null)
            {
                optionsBuilder.UseLoggerFactory(_loggerFactory);
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var provider = _options.GetDbProvider(typeof(ShellDescriptorDbContext));
            provider.OnCreateModel(modelBuilder, _options);

            base.OnModelCreating(modelBuilder);
            
            (new ShellDescriptorRecordMapping()).ApplyMapping(modelBuilder, provider);
            (new SettingRecordMapping()).ApplyMapping(modelBuilder, provider);
        }
    }
}
