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
    internal class ShellDescriptorDbContext : DefaultDbContext, INewDatabaseFlag
    {
        public ShellDescriptorDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var provider = this.GetDbProvider();
            (new ShellDescriptorRecordMapping()).ApplyMapping(modelBuilder, provider);
            (new SettingRecordMapping()).ApplyMapping(modelBuilder, provider);
        }
    }
}
