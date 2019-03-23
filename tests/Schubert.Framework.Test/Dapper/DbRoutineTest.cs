using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Schubert.Framework.Data;
using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.Dapper
{
    [Collection("dapper")]
    public class DbRoutineTest : IDisposable
    {   /// <summary>
        /// 初始化配置参数
        /// </summary>
        class DapperMetadataProviderComKeyInstance : DapperMetadataProvider<DapperCompisiteKeyEntity>
        {
            protected override void ConfigureModel(DapperMetadataBuilder<DapperCompisiteKeyEntity> builder)
            {
                builder.HasKey(d => new { d.products_id, d.language_id }).TableName("dapper_compisite_key");
            }
        }
        class TestWorkContext : WorkContext
        {
            private readonly IWorkContextStateProvider _transactionProvider = new TransactionStateProvider();
            private readonly DapperRuntime _runtime = null;
            private DapperContext _dbContext = null;
            private LoggerFactory _loggerFactory = new LoggerFactory();
            public TestWorkContext(DapperRuntime runtime)
                : base()
            {
                _runtime = runtime;
                _loggerFactory.AddDebug(LogLevel.Trace);
            }

            public override object Resolve(Type type)
            {
                if (typeof(IDatabaseContext).Equals(type))
                {
                    return _dbContext ?? (_dbContext = new DapperContext(_runtime, _loggerFactory));
                }
                return null;
            }

            public override object ResolveRequired(Type type)
            {
                if (typeof(IDatabaseContext).Equals(type))
                {
                    return _dbContext ?? (_dbContext = new DapperContext(_runtime, _loggerFactory));
                }
                throw new NotSupportedException();
            }

            protected override IEnumerable<IWorkContextStateProvider> GetStateProviders()
            {
                return new IWorkContextStateProvider[]
                {
                    _transactionProvider
                };
            }
        }
        private MySQLHelper helper;
        private DapperContext _testContext = null;
        private DapperRepository<DapperCompisiteKeyEntity> _dapper;
        public DbRoutineTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
            helper = MySQLHelper.Default;
            helper.CreateDataBaseIfNoExist("test");
            helper.CreateTableIfNoExist(new[] { "dapper_compisite_key" });
            _dapper = MockGenerator();
        }

        private DapperRepository<DapperCompisiteKeyEntity> MockGenerator()
        {
            var dbProvider = new MySqlDatabaseProvider();
            Mock<IOptions<DapperDatabaseOptions>> optionsMock = new Mock<IOptions<DapperDatabaseOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new DapperDatabaseOptions
            {
                DefaultConnectionName = "default",
                ConnectionStrings = new Dictionary<string, string>
                {
                    { "default", helper.ConnectionString }
                }
            });
            LoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddDebug(LogLevel.Trace);
            DapperRuntime rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderComKeyInstance() });
            _testContext = new DapperContext(rt, loggerFactory);
            return new DapperRepository<DapperCompisiteKeyEntity>(_testContext);
        }

        private void ClearDapperCompisiteKeyEntity()
            => helper.ExecuteSQL("delete from dapper_compisite_key", command => command.ExecuteNonQuery());
        private DapperCompisiteKeyEntity CreateDapperCompisiteKeyEntity(int products_id = 1, int language_id = 1, string products_description = "1",
            string products_name = "1", string products_short_description = "1", string products_url = "1")
        {
            var entity = new DapperCompisiteKeyEntity
            {
                language_id = language_id,
                products_description = products_description,
                products_id = products_id,
                products_name = products_name,
                products_short_description = products_short_description,
                products_url = products_url,
                products_viewed = 1
            };
            string sql = string.Format(@"insert into dapper_compisite_key(language_id,products_description,products_id,products_name,products_short_description,products_url,products_viewed) value ({0},'{1}',{2},'{3}','{4}','{5}',{6})",
entity.language_id, entity.products_description, entity.products_id, entity.products_name, entity.products_short_description, entity.products_url, entity.products_viewed);
            if (helper.ExecuteSQL(sql, cmd => cmd.ExecuteNonQuery()) > 0) return entity;
            return null;
        }
        [Fact(DisplayName = "DapperRepositoryComKey:用Lamba查询所有操作")]
        private void QueryAllUseLambda()
        {
            ClearDapperCompisiteKeyEntity();
            var d1 = CreateDapperCompisiteKeyEntity();
            var d2 = CreateDapperCompisiteKeyEntity(2, 2, "1", "2", "2", "2");
            var d3 = CreateDapperCompisiteKeyEntity(3, 2, "3", "2", "3", "3");
            try
            {
                var dapperInstance = _dapper;
                Assert.True(dapperInstance.Query(x => x.products_id == d1.products_id).Count() == 1);
                Assert.True(dapperInstance.Query(x => x.products_id == 100).Count() == 0);
                Assert.True(dapperInstance.Query(x => x.products_id == d2.products_id || x.language_id == d2.language_id).Count() == 2);
                Assert.True(dapperInstance.Query(x => x.products_id == d1.products_id && x.language_id == d1.language_id && x.products_name == d1.products_name.ToString()).Count() == 1);
                Assert.True(dapperInstance.Query(x => (x.products_id == d1.products_id && x.products_name == d1.products_name.ToString()) || x.language_id == d2.language_id).Count() == 3);
                Assert.True(dapperInstance.Query(x => (x.products_id == d1.products_id && x.language_id == d1.language_id) || (x.products_id == d2.products_id && x.language_id == d2.language_id)).Count() == 2);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:翻页查询和排序")]
        private void PageQuery()
        {
            ClearDapperCompisiteKeyEntity();
            var dapperInstance = _dapper;
            var d1 = CreateDapperCompisiteKeyEntity(1, 1, "1", "1", "3", "4");
            var d2 = CreateDapperCompisiteKeyEntity(2, 2, "1", "2", "3", "4");
            var d3 = CreateDapperCompisiteKeyEntity(3, 2, "3", "2", "3", "4");
            var d4 = CreateDapperCompisiteKeyEntity(4, 4, "5", "2", "3", "4");
            var d5 = CreateDapperCompisiteKeyEntity(5, 5, "5", "2", "3", "4");
            try
            {

                SingleQueryFilter filter = new SingleQueryFilter();
                filter.AddEqual(nameof(DapperCompisiteKeyEntity.products_url), "4");
                var result = dapperInstance.QueryPage(0, 3, filter, new SortOptions("products_id", SortOrder.Ascending));

                Assert.True(result.Count() == 3);
                Assert.True(result.FirstOrDefault().products_id == d1.products_id);
                Assert.True(result.LastOrDefault().products_id == d3.products_id);

                var resultDesc = dapperInstance.QueryPage(0, 3, filter, new SortOptions("products_id", SortOrder.Descending));

                Assert.True(resultDesc.Count() == 3);
                Assert.True(resultDesc.FirstOrDefault().products_id == d5.products_id);
                Assert.True(resultDesc.LastOrDefault().products_id == d3.products_id);

                var resultTwoPage = dapperInstance.QueryPage(1, 4, filter);
                Assert.True(resultTwoPage.Count() == 1);
                Assert.True(resultTwoPage.FirstOrDefault().products_id == d5.products_id);

                var resultTwoPageDesc = dapperInstance.QueryPage(1, 3, filter, new SortOptions("products_id", SortOrder.Descending));
                Assert.True(resultTwoPageDesc.Count() == 2);
                Assert.True(resultTwoPageDesc.FirstOrDefault().products_id == d2.products_id);

                SingleQueryFilter filter2 = new SingleQueryFilter();
                filter2.AddEqual(nameof(DapperCompisiteKeyEntity.products_id), 1);
                var resultFillter = dapperInstance.QueryPage(0, 3, filter2, new SortOptions("products_id", SortOrder.Descending));
                Assert.True(resultFillter.Count() == 1);
                Assert.True(resultFillter.FirstOrDefault().products_id == d1.products_id);


                filter2.AddEqual(nameof(DapperCompisiteKeyEntity.language_id), 0);
                var resultFillterPage = dapperInstance.QueryPage(1, 3, filter2, new SortOptions("products_id", SortOrder.Descending));
                Assert.True(resultFillterPage.Count() == 0);

            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询首个或者默认值")]
        private void QueryFirstOrDefault()
        {
            ClearDapperCompisiteKeyEntity();
            var d1 = CreateDapperCompisiteKeyEntity();
            try
            {
                var ins = _dapper;
                SingleQueryFilter filter = new SingleQueryFilter();
                filter.AddEqual("products_id", d1.products_id);
                var d2 = ins.QueryFirstOrDefault(filter);
                CompareHelper.Compare(d1, d2);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }

        }
        [Fact(DisplayName = "DapperRepositoryComKey:异步查询首个或者默认值")]
        private void QueryFirstOrDefaultAsync()
        {
            ClearDapperCompisiteKeyEntity();
            var d1 = CreateDapperCompisiteKeyEntity();
            try
            {
                var ins = _dapper;
                SingleQueryFilter filter = new SingleQueryFilter();
                filter.AddEqual("products_id", d1.products_id);
                var d2 = ins.QueryFirstOrDefaultAsync(filter).GetAwaiter().GetResult();
                CompareHelper.Compare(d1, d2);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询所有操作")]
        private void QueryAll()
        {
            ClearDapperCompisiteKeyEntity();

            CreateDapperCompisiteKeyEntity(1, 1);
            CreateDapperCompisiteKeyEntity(2, 2);
            CreateDapperCompisiteKeyEntity(3, 3);
            CreateDapperCompisiteKeyEntity(4, 4);
            try
            {
                var dapperInstance = _dapper;
                var dapperQueryResult = dapperInstance.QueryAll();
                Assert.True(dapperQueryResult.Count() == 4);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:异步查询所有操作")]
        private void QueryAllAsync()
        {
            ClearDapperCompisiteKeyEntity();
            CreateDapperCompisiteKeyEntity();
            CreateDapperCompisiteKeyEntity(2, 2);
            CreateDapperCompisiteKeyEntity(3, 3);
            CreateDapperCompisiteKeyEntity(4, 4);
            try
            {
                var dapperInstance = _dapper;
                var dapperQueryResult = dapperInstance.QueryAllAsync().GetAwaiter().GetResult();
                Assert.True(dapperQueryResult.Count() == 4);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询操作")]
        private void Query()
        {
            var dapperInstance = _dapper;
            var deleteResult = helper.GetValue("delete FROM test.dapper_compisite_key;select 1;", Convert.ToInt32);
            var insertId = helper.GetValue("SELECT ifnull((SELECT MAX(products_id) FROM test.dapper_compisite_key),0)", Convert.ToInt32) + 1;

            {
                var id = insertId;
                var sql = $"INSERT INTO test.dapper_compisite_key VALUES ({id},{id}, '{id.ToString()}', '{id.ToString()}', '{id.ToString()}', '{id.ToString()}', 2); SELECT MAX(products_id) FROM dapper_compisite_key dck;select {id};";
                helper.ExecuteSQL(sql, cmd => cmd.ExecuteNonQuery(), false);
            }

            var filter = new SingleQueryFilter().AddEqual("products_id", insertId);
            var result = dapperInstance.Query(filter) ?? new List<DapperCompisiteKeyEntity>();

            helper.ExecuteReader($"select * from dapper_compisite_key where products_id={insertId}", reader =>
            {
                while (reader.Read())
                {
                    CompareHelper.Compare(reader, result.Where(x => x.products_id == Convert.ToInt32(reader["products_id"] ?? 0)).FirstOrDefault());
                }
            });
            helper.ExecuteSQL($"delete from test.dapper_compisite_key where products_id = {insertId}", cmd => cmd.ExecuteNonQuery(), false);
        }
        [Fact(DisplayName = "DapperRepositoryComKey:异步查询操作")]
        private void QueryAsync()
        {
            var dapperInstance = _dapper;
            var id = helper.GetValue("SELECT ifnull((SELECT MAX(products_id) FROM test.dapper_compisite_key),0)", Convert.ToInt32);
            var insertId = helper.GetValue($"INSERT INTO test.dapper_compisite_key VALUES ({++id},{id}, '{id.ToString()}', '{id.ToString()}', '{id.ToString()}', '{id.ToString()}', 2); SELECT MAX(products_id) FROM dapper_compisite_key dck;select {id};", Convert.ToInt32);

            SingleQueryFilter filter = new SingleQueryFilter().AddEqual("products_id", insertId);

            var result = dapperInstance.QueryAsync(filter).GetAwaiter().GetResult() ?? new List<DapperCompisiteKeyEntity>();
            helper.ExecuteReader($"select * from dapper_compisite_key where products_id={insertId}", reader =>
            {
                while (reader.Read())
                {
                    CompareHelper.Compare(reader, result.Where(x => x.products_id == Convert.ToInt32(reader["products_id"] ?? 0)).FirstOrDefault());
                }
            });
            helper.ExecuteSQL($"delete from test.dapper_compisite_key where products_id = {insertId}", cmd => cmd.ExecuteNonQuery(), false);
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询大于操作")]
        private void QueryGreater()
        {
            ClearDapperCompisiteKeyEntity();
            CreateDapperCompisiteKeyEntity();
            CreateDapperCompisiteKeyEntity(2, 2);
            CreateDapperCompisiteKeyEntity(3, 3);
            try
            {
                var dapperInstance = _dapper;
                SingleQueryFilter filter = new SingleQueryFilter().AddGreater("products_id", 2);
                var dapperQueryResult = dapperInstance.Query(filter);
                Assert.Equal(1, dapperQueryResult.Count());
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询大于等于操作")]
        private void QueryGreaterOrEqual()
        {
            ClearDapperCompisiteKeyEntity();
            CreateDapperCompisiteKeyEntity();
            var d2 = CreateDapperCompisiteKeyEntity(2, 2);
            var d3 = CreateDapperCompisiteKeyEntity(3, 3);

            try
            {
                var dapperInstance = _dapper;

                SingleQueryFilter filter = new SingleQueryFilter().AddGreaterOrEqual("products_id", 2);
                var dapperQueryResult = dapperInstance.Query(filter);
                CompareHelper.Compare(d2, dapperQueryResult.Where(s => s.products_id == 2).FirstOrDefault());
                CompareHelper.Compare(d3, dapperQueryResult.Where(s => s.products_id == 3).FirstOrDefault());

                Assert.Equal(2, dapperQueryResult.Count());
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询小于操作")]
        private void QueryLess()
        {
            ClearDapperCompisiteKeyEntity();

            var d1 = CreateDapperCompisiteKeyEntity();
            var d2 = CreateDapperCompisiteKeyEntity(2, 2);
            var d3 = CreateDapperCompisiteKeyEntity(3, 3);

            try
            {
                var dapperInstance = _dapper;
                SingleQueryFilter filter = new SingleQueryFilter().AddLess("products_id", 2);
                var dapperQueryResult = dapperInstance.Query(filter);
                CompareHelper.Compare(d1, dapperQueryResult.Where(s => s.products_id == 1).FirstOrDefault());

                Assert.Equal(1, dapperQueryResult.Count());
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询小于等于操作")]
        private void QueryLessOrEqual()
        {
            ClearDapperCompisiteKeyEntity();

            var d1 = CreateDapperCompisiteKeyEntity();
            var d2 = CreateDapperCompisiteKeyEntity(2, 2);
            var d3 = CreateDapperCompisiteKeyEntity(3, 3);

            try
            {
                var dapperInstance = _dapper;

                SingleQueryFilter filter = new SingleQueryFilter().AddLessOrEqual("products_id", 2);
                var dapperQueryResult = dapperInstance.Query(filter);
                CompareHelper.Compare(d1, dapperQueryResult.Where(s => s.products_id == 1).FirstOrDefault());
                CompareHelper.Compare(d2, dapperQueryResult.Where(s => s.products_id == 2).FirstOrDefault());

                Assert.Equal(dapperQueryResult.Count(), 2);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询不等于操作")]
        private void QueryAddNotEqual()
        {
            ClearDapperCompisiteKeyEntity();

            var d1 = CreateDapperCompisiteKeyEntity();
            var d2 = CreateDapperCompisiteKeyEntity(2, 2);
            var d3 = CreateDapperCompisiteKeyEntity(3, 3);
            try
            {
                var dapperInstance = _dapper;

                SingleQueryFilter filter = new SingleQueryFilter().AddNotEqual("products_id", 1);
                var dapperQueryResult = dapperInstance.Query(filter);
                CompareHelper.Compare(d2, dapperQueryResult.Where(s => s.products_id == 2).FirstOrDefault());
                CompareHelper.Compare(d3, dapperQueryResult.Where(s => s.products_id == 3).FirstOrDefault());

                Assert.Equal(dapperQueryResult.Count(), 2);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询Or操作")]
        private void QueryOr()
        {
            ClearDapperCompisiteKeyEntity();

            var d1 = CreateDapperCompisiteKeyEntity();
            var d2 = CreateDapperCompisiteKeyEntity(2, 2);
            var d3 = CreateDapperCompisiteKeyEntity(3, 3);
            try
            {
                var dapperInstance = _dapper;

                SingleQueryFilter filter = new SingleQueryFilter().AddEqual("products_id", 1);

                SingleQueryFilter filter1 = new SingleQueryFilter().AddEqual("language_id", 2);

                var filter3 = QueryFilter.CombineOr(filter, filter1);

                var dapperQueryResult = dapperInstance.Query(filter3);
                CompareHelper.Compare(d1, dapperQueryResult.Where(s => s.products_id == 1).FirstOrDefault());
                CompareHelper.Compare(d2, dapperQueryResult.Where(s => s.products_id == 2).FirstOrDefault());

                Assert.Equal(dapperQueryResult.Count(), 2);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:查询情况汇总操作")]
        private void QueryCollect()
        {
            ClearDapperCompisiteKeyEntity();

            var d1 = CreateDapperCompisiteKeyEntity();
            var d2 = CreateDapperCompisiteKeyEntity(2, 2);
            var d3 = CreateDapperCompisiteKeyEntity(3, 3);
            try
            {
                var dapperInstance = _dapper;

                SingleQueryFilter filter = new SingleQueryFilter().AddGreater("products_id", 1).AddLess("products_id", 3);
                var dapperQueryResult = dapperInstance.Query(filter);
                CompareHelper.Compare(d2, dapperQueryResult.Where(s => s.products_id == 2).FirstOrDefault());
                Assert.Equal(dapperQueryResult.Count(), 1);

                SingleQueryFilter filter2 = new SingleQueryFilter().AddEqual("products_id", 1);

                var comFilter1 = QueryFilter.CombineOr(filter, filter2);
                dapperQueryResult = dapperInstance.Query(comFilter1);
                CompareHelper.Compare(d2, dapperQueryResult.Where(s => s.products_id == 2).FirstOrDefault());
                CompareHelper.Compare(d1, dapperQueryResult.Where(s => s.products_id == 1).FirstOrDefault());
                Assert.Equal(dapperQueryResult.Count(), 2);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:插入操作")]
        public void Insert()
        {
            ClearDapperCompisiteKeyEntity();
            try
            {
                var d1 = new DapperCompisiteKeyEntity
                {
                    language_id = 1,
                    products_description = "1",
                    products_id = 1,
                    products_name = "1",
                    products_short_description = "1",
                    products_url = "1",
                    products_viewed = 1
                };
                _dapper.Insert(d1);
                var sql = string.Format("select * from dapper_compisite_key where products_id = {0} and language_id = {1}", d1.products_id, d1.language_id);
                helper.ExecuteReader(sql, reader =>
                {
                    if (reader.Read())
                    {
                        CompareHelper.Compare(reader, d1);
                    }
                });
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:异步插入操作")]
        public void InsertSync()
        {
            ClearDapperCompisiteKeyEntity();
            try
            {
                var d1 = new DapperCompisiteKeyEntity
                {
                    language_id = 1,
                    products_description = "1",
                    products_id = 1,
                    products_name = "1",
                    products_short_description = "1",
                    products_url = "1",
                    products_viewed = 1
                };
                _dapper.InsertAsync(d1).GetAwaiter().GetResult();
                var sql = string.Format("select * from dapper_compisite_key where products_id = {0} and language_id = {1}", d1.products_id, d1.language_id);
                helper.ExecuteReader(sql, reader =>
                {
                    if (reader.Read())
                    {
                        CompareHelper.Compare(reader, d1);
                    }
                });
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:删除操作")]
        public void Delete()
        {
            ClearDapperCompisiteKeyEntity();
            var d1 = CreateDapperCompisiteKeyEntity();
            try
            {
                var ins = _dapper.Delete(d1);
                var sql = $"select count(*) from dapper_compisite_key where products_id = {d1.products_id} and language_id = {d1.language_id}";
                var count = helper.GetValue(sql, Convert.ToInt32);
                Assert.Equal(count, 0);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:异步删除操作")]
        public void DeleteSync()
        {
            ClearDapperCompisiteKeyEntity();
            var d1 = CreateDapperCompisiteKeyEntity();
            try
            {
                _dapper.DeleteAsync(d1).GetAwaiter().GetResult();
                var sql = $"select count(*) from dapper_compisite_key where products_id = {d1.products_id} and language_id = {d1.language_id}";
                var count = helper.GetValue(sql, Convert.ToInt32);
                Assert.Equal(count, 0);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:更新操作")]
        public void Update()
        {
            ClearDapperCompisiteKeyEntity();
            var d1 = CreateDapperCompisiteKeyEntity();
            try
            {
                d1.products_name = "testtest";
                _dapper.Update(d1);
                var sql = $"select * from dapper_compisite_key where products_id = {d1.products_id} and language_id = {d1.language_id}";
                helper.ExecuteReader(sql, reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, d1);
                    }
                });
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:异步更新操作")]
        public void UpdateSync()
        {
            ClearDapperCompisiteKeyEntity();
            var d1 = CreateDapperCompisiteKeyEntity();
            try
            {
                d1.products_name = "testtest";
                _dapper.UpdateAsync(d1).GetAwaiter().GetResult();
                var sql = $"select * from dapper_compisite_key where products_id = {d1.products_id} and language_id = {d1.language_id}";
                helper.ExecuteReader(sql, reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, d1);
                    }
                });
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:更新指定字段操作")]
        public void UpdateSpeParam()
        {
            ClearDapperCompisiteKeyEntity();
            var d1 = CreateDapperCompisiteKeyEntity();
            try
            {
                var param = new List<KeyValuePair<string, object>>
                {
                     new KeyValuePair<string, object>("products_name",$"{(d1.products_id + 100).ToString()}"),
                     new KeyValuePair<string, object>("products_description",$"{(d1.products_id + 100).ToString()}"),
                     new KeyValuePair<string, object>("products_viewed",(d1.products_id + 100))
                };

                _dapper.Update(d1, param);
                d1.products_name = (d1.products_id + 100).ToString();
                d1.products_description = (d1.products_id + 100).ToString();
                d1.products_viewed = d1.products_id + 100;
                var sql = $"select * from dapper_compisite_key where products_id = {d1.products_id} and language_id = {d1.language_id}";
                helper.ExecuteReader(sql, reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, d1);
                    }
                });
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepositoryComKey:异步更新指定字段操作")]
        public void UpdateSpeParamSync()
        {
            ClearDapperCompisiteKeyEntity();
            var d1 = CreateDapperCompisiteKeyEntity();
            try
            {
                var param = new List<KeyValuePair<string, object>>
                {
                     new KeyValuePair<string, object>("products_name",$"{(d1.products_id + 100).ToString()}"),
                     new KeyValuePair<string, object>("products_description",$"{(d1.products_id + 100).ToString()}"),
                     new KeyValuePair<string, object>("products_viewed",(d1.products_id + 100))
                };

                _dapper.UpdateAsync(d1, param).GetAwaiter().GetResult();
                d1.products_name = (d1.products_id + 100).ToString();
                d1.products_description = (d1.products_id + 100).ToString();
                d1.products_viewed = d1.products_id + 100;
                var sql = $"select * from dapper_compisite_key where products_id = {d1.products_id} and language_id = {d1.language_id}";
                helper.ExecuteReader(sql, reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, d1);
                    }
                });
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:Count")]
        public void Count()
        {
            ClearDapperCompisiteKeyEntity();
            try
            {
                CreateDapperCompisiteKeyEntity(1, 1, "2", "2", "5", "5");
                CreateDapperCompisiteKeyEntity(1, 2, "2", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(2, 2, "3", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(3, 3, "3", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(4, 3, "3", "4", "5", "2");

                int count = _dapper.Count();
                Assert.Equal(5, count);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:CountAsync")]
        public void CountAsync()
        {
            ClearDapperCompisiteKeyEntity();
            try
            {
                CreateDapperCompisiteKeyEntity(1, 1, "2", "2", "5", "5");
                CreateDapperCompisiteKeyEntity(1, 2, "2", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(2, 2, "3", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(3, 3, "3", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(4, 3, "3", "4", "5", "2");
                int count = _dapper.CountAsync().GetAwaiter().GetResult();
                Assert.Equal(5, count);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:CountFilter")]
        public void CountFilter()
        {
            ClearDapperCompisiteKeyEntity();
            try
            {
                CreateDapperCompisiteKeyEntity(1, 1, "2", "2", "5", "5");
                CreateDapperCompisiteKeyEntity(1, 2, "2", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(2, 2, "3", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(3, 3, "3", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(4, 3, "3", "4", "5", "2");

                var ins = _dapper;
                var qf = new SingleQueryFilter()
                  .AddEqual(nameof(DapperCompisiteKeyEntity.products_id), 1)
                  .AddEqual(nameof(DapperCompisiteKeyEntity.language_id), 1);
                int count = ins.Count(qf);
                Assert.Equal(1, count);

                var qf2 = new SingleQueryFilter()
                    .AddEqual(nameof(DapperCompisiteKeyEntity.products_id), 1)
                    .AddEqual(nameof(DapperCompisiteKeyEntity.language_id), 2);

                var c1 = new CombinedQueryFilter(qf, qf2, BooleanClause.Or);
                count = ins.Count(c1);
                Assert.Equal(2, count);

                var qf3 = new SingleQueryFilter().AddEqual(nameof(DapperCompisiteKeyEntity.products_description), 3);
                count = ins.Count(qf3);
                Assert.Equal(3, count);


                var qf4 = new SingleQueryFilter().AddEqual(nameof(DapperCompisiteKeyEntity.products_name), 4);
                count = ins.Count(qf4);
                Assert.Equal(4, count);

                var qf5 = new SingleQueryFilter().AddEqual(nameof(DapperCompisiteKeyEntity.products_short_description), 5);
                count = ins.Count(qf5);
                Assert.Equal(5, count);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:CountFilter")]
        public void CountFilterAsunc()
        {
            ClearDapperCompisiteKeyEntity();
            try
            {
                CreateDapperCompisiteKeyEntity(1, 1, "2", "2", "5", "5");
                CreateDapperCompisiteKeyEntity(1, 2, "2", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(2, 2, "3", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(3, 3, "3", "4", "5", "2");
                CreateDapperCompisiteKeyEntity(4, 3, "3", "4", "5", "2");

                var ins = _dapper;
                var qf = new SingleQueryFilter()
                  .AddEqual(nameof(DapperCompisiteKeyEntity.products_id), 1)
                  .AddEqual(nameof(DapperCompisiteKeyEntity.language_id), 1);
                int count = ins.CountAsync(qf).GetAwaiter().GetResult();
                Assert.Equal(1, count);

                var qf2 = new SingleQueryFilter()
                   .AddEqual(nameof(DapperCompisiteKeyEntity.products_id), 1)
                   .AddEqual(nameof(DapperCompisiteKeyEntity.language_id), 2);

                var c1 = new CombinedQueryFilter(qf, qf2, BooleanClause.Or);
                count = ins.CountAsync(c1).GetAwaiter().GetResult();
                Assert.Equal(2, count);

                var qf3 = new SingleQueryFilter().AddEqual(nameof(DapperCompisiteKeyEntity.products_description), 3);
                count = ins.CountAsync(qf3).GetAwaiter().GetResult();
                Assert.Equal(3, count);


                var qf4 = new SingleQueryFilter().AddEqual(nameof(DapperCompisiteKeyEntity.products_name), 4);
                count = ins.CountAsync(qf4).GetAwaiter().GetResult();
                Assert.Equal(4, count);

                var qf5 = new SingleQueryFilter().AddEqual(nameof(DapperCompisiteKeyEntity.products_short_description), 5);
                count = ins.CountAsync(qf5).GetAwaiter().GetResult();
                Assert.Equal(5, count);
            }
            finally
            {
                ClearDapperCompisiteKeyEntity();
            }
        }


        private WorkContext MockWorkContext(DapperRuntime rt)
        {
            return new TestWorkContext(rt);
        }
        private DapperRepository<T> GetRepository<T>(WorkContext workContext)
            where T : class
        {
            DapperContext context = workContext.Resolve<IDatabaseContext>() as DapperContext;
            return new DapperRepository<T>(context);
        }
        private DapperRuntime GetDapperRuntime()
        {
            Mock<IOptions<DapperDatabaseOptions>> optionsMock = new Mock<IOptions<DapperDatabaseOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new DapperDatabaseOptions
            {
                DefaultConnectionName = "default",
                ConnectionStrings = new Dictionary<string, string>
                {
                    { "default", helper.ConnectionString }
                }
            });

            DapperRuntime rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderComKeyInstance() });
            return rt;
        }
        [Fact(DisplayName = "DapperRepositoryComKey:DB事物测试")]
        private void DBTransactionCommitTest()
        {
            ClearDapperCompisiteKeyEntity();
            var result = helper.ExecuteSQL<object>(@"DROP TABLE IF EXISTS dapper_compisite_key", command =>
            {
                helper.CreateDataBaseIfNoExist("test");
                helper.CreateTableIfNoExist(new[] { "dapper_compisite_key" });
                var dapperRuntime = this.GetDapperRuntime();
                var workContext = this.MockWorkContext(dapperRuntime);
                var dapperRepository = this.GetRepository<DapperCompisiteKeyEntity>(workContext);
                var id = 1;
                #region 类对象
                var entity = new DapperCompisiteKeyEntity
                {
                    products_id = id + 1,
                    language_id = id + 1,
                    products_name = (id + 1).ToString(),
                    products_description = (id + 1).ToString(),
                    products_short_description = (id + 1).ToString(),
                    products_url = (id + 1).ToString(),
                    products_viewed = id + 1
                };

                var entityNext = new DapperCompisiteKeyEntity
                {
                    products_id = id + 2,
                    language_id = id + 2,
                    products_name = (id + 2).ToString(),
                    products_description = (id + 2).ToString(),
                    products_short_description = (id + 2).ToString(),
                    products_url = (id + 2).ToString(),
                    products_viewed = (id + 2)
                };
                #endregion
                try
                {
                    using (DbTransactionScope scope = new DbTransactionScope(workContext))
                    {
                        dapperRepository.Insert(entity);
                        using (DbTransactionScope scope2 = new DbTransactionScope(workContext))
                        {
                            dapperRepository.Insert(entityNext);
                            scope2.Complete();
                        }
                        scope.Complete();
                    }
                    var sql = "select count(*) from dapper_compisite_key";
                    command.CommandText = sql;
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    Assert.Equal(count, 2);
                }
                finally
                {
                    ClearDapperCompisiteKeyEntity();
                }
                return null;
            });
            Assert.Equal(result, null);
        }
        [Fact(DisplayName = "DapperRepositoryComKey:DB事物测试回滚测试")]
        private void DbTransactionRollbackTest()
        {
            ClearDapperCompisiteKeyEntity();
            var result = helper.ExecuteSQL<object>(@"DROP TABLE IF EXISTS dapper_compisite_key", command =>
              {
                  helper.CreateDataBaseIfNoExist("test");
                  helper.CreateTableIfNoExist(new[] { "dapper_compisite_key" });
                  var dapperRuntime = this.GetDapperRuntime();
                  var workContext = this.MockWorkContext(dapperRuntime);
                  var dapperRepository = this.GetRepository<DapperCompisiteKeyEntity>(workContext);
                  var id = 1;
                  #region 类对象

                  var entity = new DapperCompisiteKeyEntity()
                  {
                      products_id = id + 1,
                      language_id = id + 1,
                      products_name = (id + 1).ToString(),
                      products_description = (id + 1).ToString(),
                      products_short_description = (id + 1).ToString(),
                      products_url = (id + 1).ToString(),
                      products_viewed = id + 1
                  };

                  var entityNext = new DapperCompisiteKeyEntity()
                  {
                      products_id = id + 2,
                      language_id = id + 2,
                      products_name = (id + 2).ToString(),
                      products_description = (id + 2).ToString(),
                      products_short_description = (id + 2).ToString(),
                      products_url = (id + 2).ToString(),
                      products_viewed = (id + 2)
                  };

                  #endregion
                  try
                  {
                      using (DbTransactionScope scope = new DbTransactionScope(workContext))
                      {
                          dapperRepository.Insert(entity);
                          using (DbTransactionScope scope2 = new DbTransactionScope(workContext))
                          {
                              dapperRepository.Insert(entityNext);
                          }
                          scope.Complete();
                      }
                      command.CommandText = "select count(*) from dapper_compisite_key";
                      var count = Convert.ToInt32(command.ExecuteScalar());
                      Assert.Equal(count, 0);

                      // 2、里面的Complete,外面没有Complete，数据不提交
                      using (DbTransactionScope scope = new DbTransactionScope(workContext))
                      {
                          dapperRepository.Insert(entity);
                          using (DbTransactionScope scope2 = new DbTransactionScope(workContext))
                          {
                              dapperRepository.Insert(entityNext);
                              scope2.Complete();
                          }
                      }
                      count = Convert.ToInt32(command.ExecuteScalar());
                      Assert.Equal(count, 0);

                      // 3、插入同样主键的两条数据，提交失败回滚
                      using (DbTransactionScope scope = new DbTransactionScope(workContext))
                      {
                          dapperRepository.Insert(entity);
                          using (DbTransactionScope scope2 = new DbTransactionScope(workContext))
                          {
                              dapperRepository.Insert(entityNext);
                              scope2.Complete();
                          }
                          scope.Complete();
                      }
                      count = Convert.ToInt32(command.ExecuteScalar());
                      Assert.Equal(count, 2);
                  }
                  finally
                  {
                      ClearDapperCompisiteKeyEntity();
                  }
                  return null;
              });
        }

        public void Dispose()
        {
            _testContext?.Dispose();
        }
    }
}
