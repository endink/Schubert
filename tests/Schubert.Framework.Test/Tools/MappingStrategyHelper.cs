using Microsoft.Extensions.Options;
using Schubert.Framework.Data;
using System.Reflection;

namespace Schubert.Framework.Test.Tools
{
    static class MappingStrategyHelper
    {
        public static void SetStrategy(IdentifierMappingStrategy strategy)
        {
            var assembly = Assembly.GetAssembly(typeof(IdentifierMappingStrategy));
            var type = assembly.GetType("Schubert.Framework.Data.MappingStrategyParser");
            var field = type.GetField("_dapperDatabaseOptions", BindingFlags.Static | BindingFlags.NonPublic);
            Moq.Mock<IOptions<DapperDatabaseOptions>> mock = new Moq.Mock<IOptions<DapperDatabaseOptions>>();
            mock.SetReturnsDefault(new DapperDatabaseOptions
            {
                Dapper = new DapperOptions
                {
                    IdentifierMappingStrategy = strategy
                }
            });
            IOptions<DapperDatabaseOptions> options = mock.Object;
            field.SetValue(null, mock.Object);
        }
    }
}
