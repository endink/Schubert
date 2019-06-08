using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Schubert.Framework;
using Schubert.Framework.Data;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Environment.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Schubert
{
    public static class EFServiceCollectionExtensions
    {
        private static Guid _module = Guid.NewGuid();
        private static Dictionary<String, Type> _dbConnectionMappings = new Dictionary<string, Type>();

        public static SchubertServicesBuilder AddEntityFrameworkFeature(this SchubertServicesBuilder builder, Action<DbBuilder> setup = null)
        {
            DbOptions options = new DbOptions();
            if (builder.AddedModules.Add(_module))
            {
                var configuration = builder.Configuration.GetSection("Schubert:Data") as IConfiguration ?? new ConfigurationBuilder().Build();
                builder.ServiceCollection.Configure<DbOptions>(configuration);
                
                var schubertDataSetup = new ConfigureFromConfigurationOptions<DbOptions>(configuration);
                schubertDataSetup.Configure(options);
            }
            

            DbBuilder dbBuilder = new DbBuilder();
            if (setup != null)
            {
                setup(dbBuilder);
                if (dbBuilder.Setup != null)
                {
                    dbBuilder.Setup(options);
                    builder.ServiceCollection.Configure(dbBuilder.Setup);
                }
            }

            options.DbContextSettings = dbBuilder.DbSettings;
            builder.ServiceCollection.Configure<DbOptions>(dbOp =>
            {
                dbOp.DbContextSettings.AddRange(dbBuilder.DbSettings, true);
            });
            _dbConnectionMappings.AddRange(dbBuilder.DbContexts, true);

            if (dbBuilder.ShellDbAdded)
            {
                builder.ServiceCollection.AddSmart(ServiceDescriber.Scoped<IRepository<ShellDescriptorRecord>, Repository<ShellDescriptorRecord, ShellDescriptorDbContext>>());
                builder.ServiceCollection.AddSmart(ServiceDescriber.Scoped<IRepository<SettingRecord>, Repository<SettingRecord, ShellDescriptorDbContext>>());
                builder.ServiceCollection.AddSmart(ServiceDescriber.Scoped<IShellDescriptorManager, ShellDescriptorManager>());
            }

            builder.ServiceCollection.AddSmart(EntityFrameworkServices.GetServices(options));


            foreach (var action in dbBuilder.DbConfigurings.Values)
            {
                action(builder.ServiceCollection);
            }

            builder.ServiceCollection.AddSmart(ServiceDescriber.Scoped(
                s => 
                {
                    IOptions<DbOptions> dbOps = s.GetRequiredService<IOptions<DbOptions>>();
                    return new DbContextResources(s, dbBuilder.DefaultDbConnectionName, _dbConnectionMappings);
                },
                SmartOptions.Replace));

            builder.ServiceCollection.TryAddScoped<IDatabaseContext>(s => s.GetRequiredService<DbContextResources>());

            return builder;
        }
    }
}