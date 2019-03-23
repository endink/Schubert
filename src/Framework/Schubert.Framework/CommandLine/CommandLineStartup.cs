using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.DependencyInjection;
using System;

namespace Schubert.Framework
{
    public abstract class CommandLineStartup
    {
        protected CommandLineStartup()
        {

        }

        public IConfigurationRoot Configuration { get; protected internal set; }

        protected internal abstract void BuildConfiguration(String environment, IConfigurationBuilder builder);

        protected internal abstract void ConfigureServices(SchubertServicesBuilder builder);

        protected internal abstract void Configure(IServiceProvider serviceProvider);

        protected internal virtual void ConfigureShellCreationScope(ShellCreationScope scope)
        { }
    }
}
