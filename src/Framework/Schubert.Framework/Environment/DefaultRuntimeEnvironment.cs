
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Versioning;

namespace Schubert.Framework.Environment
{
    /// <summary>
    /// 以 DEBUG 编译符号来作为判断开发环境标准的提供程序。
    /// </summary>
    public class DefaultRuntimeEnvironment : ISchubertEnvironment
    {
        private IInstanceIdProvider _instanceIdProvider = null;

        public DefaultRuntimeEnvironment(String complieConfiguration, IInstanceIdProvider instanceIdProvider)
        {
            Guard.ArgumentNotNull(instanceIdProvider, nameof(instanceIdProvider));

            _instanceIdProvider = instanceIdProvider;
            this.IsDevelopmentEnvironment = (complieConfiguration.IfNullOrWhiteSpace(String.Empty)).CaseInsensitiveEquals("development");
            this.Environment = complieConfiguration.IfNullOrWhiteSpace("Production").ToLower();
            this.ApplicationBasePath = SchubertUtility.GetApplicationDirectory();
            this.RuntimeFramework = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;

        }

        public string ApplicationInstanceId
        {
            get { return _instanceIdProvider.GetInstanceId(); }
        }

        public string Environment { get; }

        public bool IsDevelopmentEnvironment { get; }

        public string ApplicationBasePath { get; }
        public string Configuration { get; }
        public String RuntimeFramework { get; }
    }
}
