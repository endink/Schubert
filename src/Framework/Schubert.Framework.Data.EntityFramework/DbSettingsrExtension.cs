using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schubert.Framework.Data
{
    class DbSettingsrExtension : IDbContextOptionsExtension
    {
        private DbContextSettings _settings;
        public DbSettingsrExtension(DbContextSettings settings)
        {
            Guard.ArgumentNotNull(settings, nameof(settings));
            _settings = settings;
        }

        public string LogFragment => $"use DbContextSettings";

        public bool ApplyServices(IServiceCollection services)
        {
            services.AddSingleton(_settings);
            return false;
         }

        public long GetServiceProviderHashCode() => 0;

        public void Validate(IDbContextOptions options)
        {
            var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
            if (internalServiceProvider != null)
            {
                using (var scope = internalServiceProvider.CreateScope())
                {
                    if (scope.ServiceProvider.GetService<IEnumerable<DbContextSettings>>()
                            ?.Any(s => s is DbContextSettings) != true)
                    {
                        throw new InvalidOperationException("Missing DbContextSettings.");
                    }
                }
            }
        }
    }
}
