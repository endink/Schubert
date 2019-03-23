using Microsoft.AspNetCore.Hosting;
using Schubert.Framework.Environment;
using System;
using System.Reflection;
using System.Runtime.Versioning;

namespace Schubert.Framework.Web
{
    public class AspNetEnvironment : ISchubertEnvironment
    {
        private IInstanceIdProvider _instanceIdProvider = null;
        public AspNetEnvironment(IHostingEnvironment hosting,  IInstanceIdProvider instanceIdProvider)
        {
            Guard.ArgumentNotNull(hosting, nameof(hosting));
            Guard.ArgumentNotNull(instanceIdProvider, nameof(instanceIdProvider));

            this.Environment = hosting.EnvironmentName.IfNullOrWhiteSpace("production").ToLower();
            this.IsDevelopmentEnvironment = hosting.IsDevelopment();
            this.RuntimeFramework = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
            //this.ApplicationBasePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;
            this.ApplicationBasePath = SchubertUtility.GetApplicationDirectory();

            _instanceIdProvider = instanceIdProvider;
        }

        public string Environment { get; set; }

        public string ApplicationBasePath { get; }

        public string ApplicationInstanceId
        {
            get { return _instanceIdProvider.GetInstanceId(); }
        }

        public bool IsDevelopmentEnvironment { get; }

        public String RuntimeFramework { get; }
    }
}
