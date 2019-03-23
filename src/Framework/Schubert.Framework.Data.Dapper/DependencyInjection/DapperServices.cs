using Schubert.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Schubert.Framework.DependencyInjection
{
    public static class DapperServices
    {
        public static IEnumerable<SmartServiceDescriptor> GetServices(DapperDatabaseOptions options)
        {
            Guard.ArgumentNotNull(options, nameof(options));

            yield return ServiceDescriber.Scoped<DapperContext, DapperContext> ();
            yield return ServiceDescriber.Scoped<IDatabaseContext>(s=>s.GetRequiredService<DapperContext>());
            
            yield return ServiceDescriber.Singleton<DapperRuntime, DapperRuntime>();
            //yield return ServiceDescriber.Scoped<IRepository<ShellDescriptorRecord>, Repository<ShellDescriptorRecord, ShellDescriptorDbContext>>();
            //yield return ServiceDescriber.Scoped<IRepository<SettingRecord>, Repository<SettingRecord, ShellDescriptorDbContext>>();
            yield return ServiceDescriber.Scoped(typeof(IRepository<>), typeof(DapperRepository<>)); 

            //yield return ServiceDescriber.Transient<IShellDescriptorManager, ShellDescriptorManager>();
            //yield return ServiceDescriber.Scoped<ILanguageService, LanguageService>();

        }
    }
}
