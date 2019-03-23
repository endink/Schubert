using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Schubert.Framework.FileSystem.AppData;
using Schubert.Helpers;
using System;

namespace Schubert.Framework.Environment
{
    public class DefaultInstanceIdProvider : IInstanceIdProvider
    {
        private String _instanceId;
        private IServiceProvider _serviceProvider;
        private string _appName = null;
        public DefaultInstanceIdProvider(IServiceProvider serviceProvider, IOptions<SchubertOptions> schubertOptions)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(schubertOptions, nameof(schubertOptions));

            _serviceProvider = serviceProvider;
            _appName = schubertOptions.Value.AppSystemName.IfNullOrEmpty("NULL_APPNAME");
        }
        
        public String GetInstanceId()
        {
            if (_instanceId.IsNullOrEmpty())
            {
                lock(this)
                {
                    if (_instanceId.IsNullOrEmpty())
                    {
                        IAppDataFolder folder = _serviceProvider.GetRequiredService<IAppDataFolder>();
                        string id = ToolHelper.NewShortId();
                        string fileName = $"{_appName}.instance";
                        if (!folder.FileExists(fileName))
                        {
                            folder.CreateFile(fileName, id);
                        }
                        else
                        {
                            id = folder.ReadFile(fileName);
                        }
                        _instanceId = id;
                    }
                }
            }
            return _instanceId;
        }
    }
}
