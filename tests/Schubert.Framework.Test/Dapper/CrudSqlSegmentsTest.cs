using Microsoft.Extensions.Options;
using Moq;
using Schubert.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.Dapper
{
    [Collection("dapper")]
    public class CrudSqlSegmentsTest
    {
        public CrudSqlSegmentsTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
        }
        private class TestEntity
        {
            public long Id { get; set; }

            public string Name { get; set; }
        }
        private class TestEntityMetadataProvider : DapperMetadataProvider<TestEntity>
        {
            protected override void ConfigureModel(DapperMetadataBuilder<TestEntity> builder)
            {
                builder.HasKey(b => b.Id).AutoGeneration(b => b.Id).TableName("T1");
            }
        }

        private CrudSqlSegments MockCrudSqlSegments()
        {
            var dbProvider = new MySqlDatabaseProvider();
            Mock<IOptions<DapperDatabaseOptions>> optionsMock = new Mock<IOptions<DapperDatabaseOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new DapperDatabaseOptions
            {
                DefaultConnectionName = "default",
                ConnectionStrings = new Dictionary<string, string>
                {
                    { "default", MySqlConnectionString.Value }
                }
            });
            DapperRuntime rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new TestEntityMetadataProvider() });

            CrudSqlSegments seg = new CrudSqlSegments(typeof(TestEntity), rt);
            return seg;
        }
        [Fact]
        public void InsertSqlTest()
        {
            CrudSqlSegments seg = MockCrudSqlSegments();
            string sql = seg.InsertSql;
            Assert.Equal(sql, "INSERT INTO `T1` (`name`)  VALUES (@Name)");
        }
    }
}
