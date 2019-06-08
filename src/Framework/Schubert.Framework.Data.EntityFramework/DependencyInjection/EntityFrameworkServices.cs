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
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Schubert.Framework.DependencyInjection
{
    public static class EntityFrameworkServices
    {
        public static IEnumerable<SmartServiceDescriptor> GetServices(DbOptions options)
        {
            Guard.ArgumentNotNull(options, nameof(options));
            
            yield return ServiceDescriber.Scoped(typeof(IRepository<>), typeof(Repository<>));
            yield return ServiceDescriber.Scoped(typeof(IRepository<,>), typeof(Repository<,>));
            yield return ServiceDescriber.Scoped(typeof(IMigrationsAssembly), typeof(MigrationFinder), SmartOptions.Replace);

            yield return ServiceDescriber.Singleton(typeof(IModelCustomizer), typeof(SchubertModelCustomizer), SmartOptions.Append);
            yield return ServiceDescriber.Singleton(typeof(ICoreConventionSetBuilder), typeof(SchubertConventionSetBuilder), SmartOptions.Append);

            yield return ServiceDescriber.Scoped<ILanguageService, LanguageService>();

        }
    }
}
