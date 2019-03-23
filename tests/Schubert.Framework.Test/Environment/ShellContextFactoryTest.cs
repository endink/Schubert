using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Environment.ShellBuilders;
using System;
using Xunit;

namespace Schubert.Framework.Test.Environment
{
    [Collection("dapper")] 
    public class ShellContextFactoryTest
    {
        [Fact(DisplayName = "ShellContextFactory：上下文基础测试")]
        public void CreateFactory()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();

            var collection = new ServiceCollection();
            collection.AddSchubertFramework(configuration);
            var provider = collection.BuildServiceProvider();

            var factory = provider.GetService<IShellContextFactory>();
            factory.CreateShellContext();
        }
    }
}
