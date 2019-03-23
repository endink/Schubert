using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.Data;
using Schubert.Framework.Environment;
using Schubert.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Configuration
{
    public class DbConfigurationProvider : JsonContentConfigurationProvider
    {
        private string _region = null;

        public DbConfigurationProvider(string region = "global", string initJsonContent = null)
            : base(initJsonContent)
        {
            _region = region.IfNullOrWhiteSpace("global");
        }

        public override void Load()
        {
            this.Data = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);
            if (SchubertEngine.Current.IsRunning)
            {
                using (IServiceScope scope = SchubertEngine.Current.CreateScope())
                {
                    IRepository<SettingRecord> repository = null;

                    repository = scope.ServiceProvider.GetRequiredService<IRepository<SettingRecord>>();
                    base.Load();
                    if (!this.Data.IsNullOrEmpty())
                    {
                        DbConfigurationHelper.WirteData(repository, this.Data, _region);
                    }
                    this.Data = DbConfigurationHelper.ReadData(repository, _region);
                }
            }
            else
            {
                base.Load();
                ShellEvents.EngineStarted += ShellEvents_OnEngineStarted;
            }
        }

        private void ShellEvents_OnEngineStarted(SchubertOptions options, IServiceProvider obj)
        {
            this.Load();
        }
    }
}
