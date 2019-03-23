using Microsoft.AspNetCore.Builder;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Web.DependencyInjection;

namespace Schubert.Framework.Web
{
    /// <summary>
    /// 表示一个  Web 启动器项。
    /// </summary>
    public abstract class WebStarter
    {
        public abstract void ConfigureServices(SchubertServicesBuilder servicesBuilder, SchubertWebOptions options);

        public abstract void Start(IApplicationBuilder appBuilder, SchubertWebOptions options);
    }
}
