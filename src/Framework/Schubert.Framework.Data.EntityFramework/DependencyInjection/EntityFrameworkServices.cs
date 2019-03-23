using Schubert.Framework.Data.Services;
using Schubert.Framework.Data;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Environment.Modules;
using Schubert.Framework.Services;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Schubert.Framework.Environment;
using Schubert.Framework.Data.EntityFramework;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Schubert.Framework.Data.Providers;

namespace Schubert.Framework.DependencyInjection
{
    public static class EntityFrameworkServices
    {
        public static IEnumerable<SmartServiceDescriptor> GetServices(DbOptions options)
        {
            Guard.ArgumentNotNull(options, nameof(options));
            
            yield return ServiceDescriber.Singleton<IDatabaseInitializer, DatabaseInitializer>();
            yield return ServiceDescriber.Scoped(typeof(IRepository<>), typeof(Repository<>));
            yield return ServiceDescriber.Scoped(typeof(IRepository<,>), typeof(Repository<,>));
            yield return ServiceDescriber.Singleton<IDatabaseInitializer, DatabaseInitializer>();
            yield return ServiceDescriber.Scoped(typeof(IMigrationsAssembly), typeof(MigrationFinder), SmartOptions.Replace);
            yield return ServiceDescriber.Singleton(typeof(ICoreConventionSetBuilder), typeof(SchubertConventionSetBuilder), SmartOptions.Replace);

            yield return ServiceDescriber.Scoped<ILanguageService, LanguageService>();

        }
    }
}
