
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.Data;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;
using Dapper;
using System.Linq.Expressions;

[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]

namespace Schubert.Framework.Test.Dapper
{

    public class DapperSqlGeneratorTest
    {
        public DapperSqlGeneratorTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
        }
        class DapperMetadataProviderInstance : DapperMetadataProvider<DapperEntityWithNoBool>
        {
            protected override void ConfigureModel(DapperMetadataBuilder<DapperEntityWithNoBool> builder)
            {
                builder.HasKey(d => d.id)
                    .TableName("dapper_all_test")
                    .DataSource("default");
            }
        }
        private class User
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private DapperSqlGenerator MockGenerator()
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

            DapperRuntime rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderInstance() });

            return new DapperSqlGenerator(rt);
        }

        [Fact(DisplayName = "DapperSqlGenerator: 简单过滤器解析")]
        private void SingleFilterTest()
        {
            DapperSqlGenerator generator = this.MockGenerator();
            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual("name", "evalue");
            filter.AddGreaterOrEqual("gte", 1);
            filter.AddGreater("gt", 0);
            filter.AddLess("lt", 10000);
            filter.AddLessOrEqual("lte", 999);
            filter.AddNotEqual("null", null);

            DynamicParameters parameters = new DynamicParameters();
            generator.GenerateFilter<DapperEntityWithNoBool>(filter, parameters);

            Assert.Equal(5, parameters.ParameterNames.Count()); // null 不作为参数。
        }

        [Fact(DisplayName = "DapperSqlGenerator: 组合过滤器解析")]
        private void CombinedFilterTest()
        {
            DapperSqlGenerator generator = this.MockGenerator();
            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual("name", "evalue");
            filter.AddGreaterOrEqual("gte", 1);
            filter.AddGreater("gt", 0);
            filter.AddLess("lt", 10000);
            filter.AddLessOrEqual("lte", 999);
            filter.AddNotEqual("null", null);



            SingleQueryFilter filter2 = new SingleQueryFilter();
            filter2.AddGreaterOrEqual("gte2", 1);
            filter2.AddEqual("name2", "evalue");
            filter2.AddGreater("gt2", 0);
            filter2.AddLess("lt2", 10000);
            filter2.AddLessOrEqual("lte2", 999);
            filter2.AddNotEqual("null2", null);

            SingleQueryFilter filter3 = new SingleQueryFilter();
            filter3.AddEqual("name23", "evalue");
            filter3.AddGreaterOrEqual("gte3", 1);
            filter3.AddGreater("gt3", 0);
            filter3.AddLess("lt3", 10000);
            filter3.AddLessOrEqual("lte3", 999);
            filter3.AddEqual("null3", null);

            CombinedQueryFilter cf1 = new CombinedQueryFilter(filter2, filter3, BooleanClause.And);

            CombinedQueryFilter cf2 = new CombinedQueryFilter(filter, cf1, BooleanClause.Or);

            DynamicParameters parameters = new DynamicParameters();
            generator.GenerateFilter<DapperEntityWithNoBool>(cf2, parameters);

            Assert.Equal(15, parameters.ParameterNames.Count()); // null 不作为参数。
        }
    }
}
