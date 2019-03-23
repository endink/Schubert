using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Schubert.Framework.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.Dapper
{
    [Collection("dapper")]
    public class DapperRepositoryTest : IDisposable
    {
        class DapperMetadataProviderInstance : DapperMetadataProvider<DapperTestEntity>
        {
            protected override void ConfigureModel(DapperMetadataBuilder<DapperTestEntity> builder)
            {
                builder.HasKey(d => d.id).TableName("dapper_all_test");
            }
        }
        class TimeTest
        {
            public int Id { get; set; }
            public DateTime Time { get; set; }
            public string User { get; set; }
        }
        class DapperMetadataProviderInstance1 : DapperMetadataProvider<TimeTest>
        {
            protected override void ConfigureModel(DapperMetadataBuilder<TimeTest> builder)
            {
                builder.HasKey(d => d.Id)
                    .AutoGeneration(d => d.Id)
                    .Ignore(d => d.Time)
                    .TableName("time_test");
            }
        }
        private DapperContext _testContext = null;
        private DapperRepository<DapperTestEntity> _dapper;
        private DapperRepository<TimeTest> _dapper1;
        private MySQLHelper helper;
        public DapperRepositoryTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
            helper = MySQLHelper.Default;
            helper.CreateDataBaseIfNoExist("test");
            helper.CreateTableIfNoExist(new[] { "dapper_all_test1", "time_test" });
            MockGenerator();
        }
        private void MockGenerator()
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
            LoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddDebug(LogLevel.Trace);
            DapperRuntime rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderInstance(), new DapperMetadataProviderInstance1() });
            _testContext = new DapperContext(rt, loggerFactory);
            _dapper = new DapperRepository<DapperTestEntity>(_testContext);
            _dapper1 = new DapperRepository<TimeTest>(_testContext);
        }
        private void ClearDapperTestEntity()
             => helper.ExecuteSQL("delete from dapper_all_test", command => command.ExecuteNonQuery());
        private DapperTestEntity CreateDapperTestEntity(int id = 1)
        {
            var entity = new DapperTestEntity
            {
                bigint_value = 1,
                bit_value = true,
                bool_value = true,
                boolean_value = true,
                date_value = DateTime.Now,
                char_null_value = null,
                dec_value = 1,
                decimal_value = 1,
                double_value = 1,
                fix_value = 1,
                float_value = 1,
                int_value = 1,
                integer_value = 1,
                longtext_null_value = null,
                mediumint_null_value = null,
                nchar_null_value = null,
                nvarchar_null_value = null,
                mediumint_value = 1,
                numeric_value = 1,
                real_value = 1,
                tinytext_null_value = null,
                smallint_value = 1,
                text_null_value = "",
                tinyint_value = 1,
                id = id,
                datetime_value = new DateTime(2016, 12, 12, 4, 4, 4)
            };
            var sql = string.Format(@"
                        insert into dapper_all_test(
                        bigint_value,bit_value,bool_value,boolean_value,date_value,dec_value,decimal_value,double_value,fix_value,float_value,int_value,integer_value,
                        mediumint_value,numeric_value,real_value,smallint_value,text_null_value,tinyint_value,id,datetime_value) 
                        value ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},'{16}',{17},{18},'{19}')",
                     entity.bigint_value, entity.bit_value == true ? 1 : 0, entity.bool_value == true ? 1 : 0, entity.boolean_value == true ? 1 : 0, entity.date_value.ToString("yyyy-MM-dd HH:mm:ss"), entity.dec_value,
                     entity.decimal_value, entity.double_value, entity.fix_value, entity.float_value, entity.int_value, entity.integer_value, entity.mediumint_value, entity.numeric_value, entity.real_value, entity.smallint_value, entity.text_null_value
                     , entity.tinyint_value, entity.id, entity.datetime_value.ToString("yyyy-MM-dd HH:mm:ss"));
            if (helper.ExecuteSQL(sql, cmd => cmd.ExecuteNonQuery()) > 0) return entity;
            return null;
        }
        [Fact(DisplayName = "DapperRepository:查询首个或者默认值")]
        private void QueryFirstOrDefault()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual("id", d1.id);
                var result = dapperInstance.QueryFirstOrDefault(filter);
                var sql = string.Format("select * from dapper_all_test where id={0}", d1.id);
                helper.ExecuteReader(sql, reader =>
                {
                    if (reader.Read())
                    {
                        CompareHelper.Compare(reader, result);
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:异步查询首个或者默认值")]
        private void QueryFirstOrDefaultAsync()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();
            try
            {
                var filter = new SingleQueryFilter().AddEqual("id", d1.id);
                var result = _dapper.QueryFirstOrDefaultAsync(filter).GetAwaiter().GetResult();
                var sql = string.Format("select * from dapper_all_test where id={0}", d1.id);
                helper.ExecuteReader(sql, reader =>
                {
                    if (reader.Read())
                    {
                        CompareHelper.Compare(reader, result);
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:查询所有操作")]
        private void QueryAll()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();
            var d2 = CreateDapperTestEntity(2);
            var d3 = CreateDapperTestEntity(3);
            try
            {
                var p = _dapper.QueryFirstOrDefault(m => m.char_null_value == "x" || m.varchar_null_value == "y");
                var result = _dapper.QueryAll() ?? new List<DapperTestEntity>();
                var sql = "select * from dapper_all_test";
                helper.ExecuteReader(sql, reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.Where(x => x.id == Convert.ToInt32(reader["id"])).FirstOrDefault());
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:异步查询所有操作")]
        private void QueryAllAsync()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();
            var d2 = CreateDapperTestEntity(2);
            var d3 = CreateDapperTestEntity(3);
            try
            {
                var result = _dapper.QueryAllAsync().GetAwaiter().GetResult() ?? new List<DapperTestEntity>();
                var sql = "select * from dapper_all_test";
                helper.ExecuteReader(sql, reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.Where(x => x.id == Convert.ToInt32(reader["id"])).FirstOrDefault());
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:查询操作")]
        private void Query()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();
            var d2 = CreateDapperTestEntity(2);
            var d3 = CreateDapperTestEntity(3);
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual(nameof(DapperTestEntity.dec_value), 1);
                var result = dapperInstance.Query(filter);
                helper.ExecuteReader("select * from dapper_all_test where id=1", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.Where(x => x.id == Convert.ToInt32(reader["id"])).FirstOrDefault());
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:异步查询操作")]
        private void QueryAsync()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();
            var d2 = CreateDapperTestEntity(2);
            var d3 = CreateDapperTestEntity(3);
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual(nameof(DapperTestEntity.dec_value), 1);
                var result = dapperInstance.QueryAsync(filter).GetAwaiter().GetResult();
                helper.ExecuteReader("select * from dapper_all_test where id=1", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.Where(x => x.id == Convert.ToInt32(reader["id"])).FirstOrDefault());
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:插入操作")]
        public void Insert()
        {
            var dapperInstance = _dapper;

            #region 实体对象赋值
            var entity = new DapperTestEntity()
            {
                bigint_value = 1,
                bit_value = true,
                bool_value = true,
                boolean_value = true,
                date_value = DateTime.Now.Date,
                char_null_value = null,
                dec_value = 1,
                decimal_value = 1,
                double_value = 1,
                fix_value = 1,
                float_value = 1,
                int_value = 1,
                integer_value = 1,
                longtext_null_value = null,
                mediumint_value = 1,
                mediumint_null_value = null,
                nchar_null_value = null,
                nvarchar_null_value = null,
                numeric_value = 1,
                real_value = 1,
                smallint_value = 1,
                text_null_value = "",
                tinytext_null_value = null,
                tinyint_value = 1,
                varbinary_null_value = new byte[] { 0x01, 0x02, 0x03 },
                binary_null_value = new byte[] { 0x01, 0x02, 0x03 },
                id = 1
            };
            #endregion

            try
            {
                ClearDapperTestEntity();

                //Dapper插入对象
                dapperInstance.Insert(entity);
                string sql = string.Format("select * from dapper_all_test where id = '{0}'", entity.id);
                helper.ExecuteReader("select * from dapper_all_test where id=1", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, entity);
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:批量插入操作")]
        public void InsertAll()
        {
            var dapperInstance = _dapper;
            #region 实体对象赋值
            var build = new Func<int, DapperTestEntity>(x => new DapperTestEntity()
            {
                bigint_value = 1,
                bit_value = true,
                bool_value = true,
                boolean_value = true,
                date_value = DateTime.Now.Date,
                char_null_value = null,
                dec_value = 1,
                decimal_value = 1,
                double_value = 1,
                fix_value = 1,
                float_value = 1,
                int_value = 1,
                integer_value = 1,
                longtext_null_value = null,
                mediumint_value = 1,
                mediumint_null_value = null,
                nchar_null_value = null,
                nvarchar_null_value = null,
                numeric_value = 1,
                real_value = 1,
                smallint_value = 1,
                text_null_value = "",
                tinytext_null_value = null,
                tinyint_value = 1,
                varbinary_null_value = new byte[] { 0x01, 0x02, 0x03 },
                binary_null_value = new byte[] { 0x01, 0x02, 0x03 },
                id = x
            });
            #endregion

            try
            {
                ClearDapperTestEntity();

                //Dapper插入对象
                var count = dapperInstance.Insert(new[]
                     {
                    build(1),
                    build(2),
                    build(3),
                    build(4),
                    build(5)
                });
                Assert.Equal(count, 5);
                var value = helper.GetValue("select count(1) from dapper_all_test", Convert.ToInt32);
                Assert.Equal(value, 5);
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:插入操作并返回id")]
        public void InsertAndReturnId()
        {
            var dapperInstance = _dapper1;
            Assert.Throws<SchubertException>(() =>
          {
              try
              {
                  ClearDapperTestEntity();
                  var test = new TimeTest { User = "admin" };
                  var x = dapperInstance.Insert(test);
                  Assert.True(x > 0);
                  Assert.True(test.Id > 0);
              }
              finally
              {
                  ClearDapperTestEntity();
              }
          });
        }
        [Fact(DisplayName = "DapperRepository:异步插入操作")]
        public void InsertSync()
        {
            var dapperInstance = _dapper;
            ClearDapperTestEntity();
            #region 实体对象赋值
            var entity = new DapperTestEntity()
            {
                bigint_value = 1,
                bit_value = true,
                bool_value = true,
                boolean_value = true,
                date_value = DateTime.Now.Date,
                char_null_value = null,
                dec_value = 1,
                decimal_value = 1,
                double_value = 1,
                fix_value = 1,
                float_value = 1,
                int_value = 1,
                integer_value = 1,
                longtext_null_value = null,
                mediumint_value = 1,
                mediumint_null_value = null,
                nchar_null_value = null,
                nvarchar_null_value = null,
                numeric_value = 1,
                real_value = 1,
                smallint_value = 1,
                text_null_value = "",
                tinytext_null_value = null,
                tinyint_value = 1,
                varbinary_null_value = new byte[] { 0x01, 0x02, 0x03 },
                binary_null_value = new byte[] { 0x01, 0x02, 0x03 },
                id = 1
            };
            #endregion

            try
            {
                //Dapper插入对象
                dapperInstance.InsertAsync(entity).GetAwaiter().GetResult();
                string sql = string.Format("select * from dapper_all_test where id = '{0}'", entity.id);
                helper.ExecuteReader("select * from dapper_all_test where id=1", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, entity);
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:删除操作")]
        public void Delete()
        {
            ClearDapperTestEntity();
            try
            {
                var d1 = CreateDapperTestEntity();
                _dapper.Delete(new DapperTestEntity { id = d1.id });
                var sql = string.Format("select count(*) from dapper_all_test where id = {0}", d1.id);
                int count = helper.GetValue(sql, Convert.ToInt32);
                Assert.Equal(count, 0);
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:异步删除操作")]
        public void DeleteSync()
        {
            ClearDapperTestEntity();
            try
            {
                var d1 = CreateDapperTestEntity();
                _dapper.DeleteAsync(new DapperTestEntity { id = d1.id }).GetAwaiter().GetResult();
                var sql = string.Format("select count(*) from dapper_all_test where id = {0}", d1.id);
                int count = helper.GetValue(sql, Convert.ToInt32);
                Assert.Equal(count, 0);
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:更新操作")]
        public void Update()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();

            try
            {
                d1.bigint_value = 1;
                d1.bit_value = true;
                d1.bool_value = true;
                d1.boolean_value = true;
                d1.date_value = DateTime.Now.AddDays(1);
                d1.char_null_value = null;
                d1.dec_value = 8888;
                d1.decimal_value = 8888;
                d1.double_value = 888;
                d1.fix_value = 8881;
                d1.float_value = 888;
                d1.int_value = 888;
                d1.integer_value = 8888;
                d1.longtext_null_value = null;
                d1.mediumint_value = 8888;
                d1.mediumint_null_value = null;
                d1.nchar_null_value = null;
                d1.nvarchar_null_value = null;
                d1.numeric_value = 8888;
                d1.real_value = 8888;
                d1.smallint_value = 8888;
                d1.text_null_value = "";
                d1.tinytext_null_value = null;
                d1.tinyint_value = 127;
                d1.varbinary_null_value = new byte[] { 0x01, 0x02, 0x03 };
                d1.binary_null_value = new byte[] { 0x01, 0x02, 0x03 };

                _dapper.Update(d1);
                string sql = string.Format("select * from dapper_all_test where id = {0}", d1.id);
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
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperRepository:异步更新操作")]
        public void UpdateSync()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();

            try
            {
                d1.bigint_value = 1;
                d1.bit_value = true;
                d1.bool_value = true;
                d1.boolean_value = true;
                d1.date_value = DateTime.Now.AddDays(1);
                d1.char_null_value = null;
                d1.dec_value = 8888;
                d1.decimal_value = 8888;
                d1.double_value = 888;
                d1.fix_value = 8881;
                d1.float_value = 888;
                d1.int_value = 888;
                d1.integer_value = 8888;
                d1.longtext_null_value = null;
                d1.mediumint_value = 8888;
                d1.mediumint_null_value = null;
                d1.nchar_null_value = null;
                d1.nvarchar_null_value = null;
                d1.numeric_value = 8888;
                d1.real_value = 8888;
                d1.smallint_value = 8888;
                d1.text_null_value = "";
                d1.tinytext_null_value = null;
                d1.tinyint_value = 127;
                d1.varbinary_null_value = new byte[] { 0x01, 0x02, 0x03 };
                d1.binary_null_value = new byte[] { 0x01, 0x02, 0x03 };

                _dapper.UpdateAsync(d1).GetAwaiter().GetResult();
                string sql = string.Format("select * from dapper_all_test where id = {0}", d1.id);
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
                ClearDapperTestEntity();
            }
        }

        [Fact(DisplayName = "DapperRepository:分页查询")]
        public void QueryPage()
        {
            ClearDapperTestEntity();
            try
            {
                const int length = 50;
                for (int i = 0; i < length; i++)
                {
                    CreateDapperTestEntity(i + 1);
                }
                var count = _dapper.QueryPage(2, 10).ToArray().Length;
                Assert.Equal(count, 10);
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        public void Dispose()
        {
            _testContext?.Dispose();
        }
    }
}
