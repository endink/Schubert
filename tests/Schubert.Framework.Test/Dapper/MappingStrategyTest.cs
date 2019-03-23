using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Schubert.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.Abstractions;

namespace Schubert.Framework.Test.Dapper
{
    [Collection("dapper")]
    public class MappingStrategyTest : IDisposable
    {
        class DapperMetadataProviderInstance : DapperMetadataProvider<DapperAllTest>
        {
            protected override void ConfigureModel(DapperMetadataBuilder<DapperAllTest> builder)
            {
                builder.HasKey(d => d.Id);
            }
        }
        private MySQLHelper helper;
        private DapperContext _testContext = null;
        private DapperRepository<DapperAllTest> _dapper;
        public MappingStrategyTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
            helper = MySQLHelper.Default;
            helper.CreateDataBaseIfNoExist("test");
            helper.CreateTableIfNoExist(new[] { "dapper_all_test" });
            _dapper = MockGenerator();
        }
        private DapperRepository<DapperAllTest> MockGenerator()
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
            DapperRuntime rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderInstance() });
            _testContext = new DapperContext(rt, loggerFactory);
            return new DapperRepository<DapperAllTest>(_testContext);
        }
        private void ClearTestEntity() => helper.ExecuteSQL("delete from dapper_all_test", command => command.ExecuteNonQuery());
        private DapperAllTest CreateTestEntity(int id = 1)
        {
            var entity = new DapperAllTest
            {
                BigintValue = 1,
                BitValue = true,
                BoolValue = true,
                BooleanValue = true,
                DateValue = DateTime.Now,
                CharNullValue = null,
                DecValue = 1,
                DecimalValue = 1,
                DoubleValue = 1,
                FixValue = 1,
                FloatValue = 1,
                IntValue = 1,
                IntegerValue = 1,
                LongtextNullValue = null,
                MediumintNullValue = null,
                NcharNullValue = null,
                NvarcharNullValue = null,
                MediumintValue = 1,
                NumericValue = 1,
                RealValue = 1,
                TinytextNullValue = null,
                SmallintValue = 1,
                TextNullValue = "",
                TinyintValue = 1,
                Id = id,
                DatetimeValue = new DateTime(2016, 12, 12, 4, 4, 4)
            };
            var sql = string.Format(@"
                        insert into dapper_all_test(
                        bigint_value,bit_value,bool_value,boolean_value,date_value,dec_value,decimal_value,double_value,fix_value,float_value,int_value,integer_value,
                        mediumint_value,numeric_value,real_value,smallint_value,text_null_value,tinyint_value,id,datetime_value) 
                        value ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},'{16}',{17},{18},'{19}')",
                     entity.BigintValue, entity.BitValue == true ? 1 : 0, entity.BoolValue == true ? 1 : 0, entity.BooleanValue == true ? 1 : 0, entity.DateValue.ToString("yyyy-MM-dd HH:mm:ss"), entity.DecValue,
                     entity.DecimalValue, entity.DoubleValue, entity.FixValue, entity.FloatValue, entity.IntValue, entity.IntegerValue, entity.MediumintValue, entity.NumericValue, entity.RealValue, entity.SmallintValue, entity.TextNullValue
                     , entity.TinyintValue, entity.Id, entity.DatetimeValue.ToString("yyyy-MM-dd HH:mm:ss"));
            if (helper.ExecuteSQL(sql, cmd => cmd.ExecuteNonQuery()) > 0) return entity;
            return null;
        }
        [Fact(DisplayName = " MappingStrategy:查询首个或者默认值")]
        private void QueryFirstOrDefault()
        {
            ClearTestEntity();
            var d1 = CreateTestEntity();
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual("id", d1.Id);
                var result = dapperInstance.QueryFirstOrDefault(filter);
                var sql = string.Format("select * from dapper_all_test where id={0}", d1.Id);
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
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:异步查询首个或者默认值")]
        private void QueryFirstOrDefaultAsync()
        {
            ClearTestEntity();
            var d1 = CreateTestEntity();
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual("id", d1.Id);
                var result = dapperInstance.QueryFirstOrDefaultAsync(filter).GetAwaiter().GetResult();
                var sql = string.Format("select * from dapper_all_test where id={0}", d1.Id);
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
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:查询所有操作")]
        private void QueryAll()
        {
            ClearTestEntity();
            var d1 = CreateTestEntity();
            var d2 = CreateTestEntity(2);
            var d3 = CreateTestEntity(3);
            try
            {
                var dapperInstance = _dapper;
                var result = dapperInstance.QueryAll();
                helper.ExecuteReader("select * from dapper_all_test", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.Where(x => x.Id == Convert.ToInt32(reader["id"] ?? 0)).FirstOrDefault());
                    }
                });
            }
            finally
            {
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:异步查询所有操作")]
        private void QueryAllAsync()
        {
            ClearTestEntity();
            var d1 = CreateTestEntity();
            var d2 = CreateTestEntity(2);
            var d3 = CreateTestEntity(3);
            try
            {
                var dapperInstance = _dapper;
                var result = dapperInstance.QueryAllAsync().GetAwaiter().GetResult();
                helper.ExecuteReader("select * from dapper_all_test", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.Where(x => x.Id == Convert.ToInt32(reader["id"] ?? 0)).FirstOrDefault());
                    }
                });
            }
            finally
            {
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:异步查询所有操作")]
        private void Query()
        {
            ClearTestEntity();
            var d1 = CreateTestEntity();
            var d2 = CreateTestEntity(2);
            var d3 = CreateTestEntity(3);
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual(nameof(DapperTestEntity.dec_value), 1);
                var result = dapperInstance.Query(filter);
                helper.ExecuteReader("select * from dapper_all_test where id=1", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.Where(x => x.Id == Convert.ToInt32(reader["id"])).FirstOrDefault());
                    }
                });
            }
            finally
            {
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:异步查询操作")]
        private void QueryAsync()
        {
            ClearTestEntity();
            var d1 = CreateTestEntity();
            var d2 = CreateTestEntity(2);
            var d3 = CreateTestEntity(3);
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual(nameof(DapperTestEntity.dec_value), 1);
                var result = dapperInstance.QueryAsync(filter).GetAwaiter().GetResult();
                helper.ExecuteReader("select * from dapper_all_test where id=1", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.Where(x => x.Id == Convert.ToInt32(reader["id"])).FirstOrDefault());
                    }
                });
            }
            finally
            {
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:插入操作")]
        public void Insert()
        {
            ClearTestEntity();
            var dapperInstance = _dapper;
            #region 实体对象赋值
            var entity = new DapperAllTest()
            {
                BigintValue = 1,
                BitValue = true,
                BoolValue = true,
                BooleanValue = true,
                DateValue = DateTime.Now.Date,
                CharNullValue = null,
                DecValue = 1,
                DecimalValue = 1,
                DoubleValue = 1,
                FixValue = 1,
                FloatValue = 1,
                IntValue = 1,
                IntegerValue = 1,
                LongtextNullValue = null,
                MediumintValue = 1,
                MediumintNullValue = null,
                NcharNullValue = null,
                NvarcharNullValue = null,
                NumericValue = 1,
                RealValue = 1,
                SmallintValue = 1,
                TextNullValue = "",
                TinytextNullValue = null,
                TinyintValue = 1,
                VarbinaryNullValue = new byte[] { 0x01, 0x02, 0x03 },
                BinaryNullValue = new byte[] { 0x01, 0x02, 0x03 },
                Id = 1
            };
            #endregion

            try
            {
                //Dapper插入对象
                dapperInstance.Insert(entity);
                string sql = string.Format("select * from dapper_all_test where id = '{0}'", entity.Id);
                helper.ExecuteReader(sql, reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, entity);
                    }
                });
            }
            finally
            {
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:异步插入操作")]
        public void InsertSync()
        {
            ClearTestEntity();
            var dapperInstance = _dapper;

            #region 实体对象赋值
            var entity = new DapperAllTest()
            {
                BigintValue = 1,
                BitValue = true,
                BoolValue = true,
                BooleanValue = true,
                DateValue = DateTime.Now.Date,
                CharNullValue = null,
                DecValue = 1,
                DecimalValue = 1,
                DoubleValue = 1,
                FixValue = 1,
                FloatValue = 1,
                IntValue = 1,
                IntegerValue = 1,
                LongtextNullValue = null,
                MediumintValue = 1,
                MediumintNullValue = null,
                NcharNullValue = null,
                NvarcharNullValue = null,
                NumericValue = 1,
                RealValue = 1,
                SmallintValue = 1,
                TextNullValue = "",
                TinytextNullValue = null,
                TinyintValue = 1,
                VarbinaryNullValue = new byte[] { 0x01, 0x02, 0x03 },
                BinaryNullValue = new byte[] { 0x01, 0x02, 0x03 },
                Id = 1
            };
            #endregion

            try
            {
                //Dapper插入对象
                dapperInstance.InsertAsync(entity).GetAwaiter().GetResult();
                string sql = string.Format("select * from dapper_all_test where id = '{0}'", entity.Id);
                helper.ExecuteReader(sql, reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, entity);
                    }
                });
            }
            finally
            {
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:删除操作")]
        public void Delete()
        {
            ClearTestEntity();
            try
            {
                var d1 = CreateTestEntity();
                _dapper.Delete(new DapperAllTest { Id = d1.Id });
                var sql = string.Format("select count(*) from dapper_all_test where id = {0}", d1.Id);
                int count = helper.GetValue(sql, Convert.ToInt32);
                Assert.Equal(count, 0);
            }
            finally
            {
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:异步删除操作")]
        public void DeleteSync()
        {
            ClearTestEntity();
            try
            {
                var d1 = CreateTestEntity();
                _dapper.DeleteAsync(new DapperAllTest { Id = d1.Id }).GetAwaiter().GetResult();
                var sql = string.Format("select count(*) from dapper_all_test where id = {0}", d1.Id);
                int count = helper.GetValue(sql, Convert.ToInt32);
                Assert.Equal(count, 0);
            }
            finally
            {
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:更新操作")]
        public void Update()
        {
            ClearTestEntity();
            var d1 = CreateTestEntity();

            try
            {
                d1.BigintValue = 1;
                d1.BitValue = true;
                d1.BoolValue = true;
                d1.BooleanValue = true;
                d1.DateValue = DateTime.Now.AddDays(1);
                d1.CharNullValue = null;
                d1.DecValue = 8888;
                d1.DecimalValue = 8888;
                d1.DoubleValue = 888;
                d1.FixValue = 8881;
                d1.FloatValue = 888;
                d1.IntValue = 888;
                d1.IntegerValue = 8888;
                d1.LongtextNullValue = null;
                d1.MediumintValue = 8888;
                d1.MediumintNullValue = null;
                d1.NcharNullValue = null;
                d1.NvarcharNullValue = null;
                d1.NumericValue = 8888;
                d1.RealValue = 8888;
                d1.SmallintValue = 8888;
                d1.TextNullValue = "";
                d1.TinytextNullValue = null;
                d1.TinyintValue = 127;
                d1.VarbinaryNullValue = new byte[] { 0x01, 0x02, 0x03 };
                d1.BinaryNullValue = new byte[] { 0x01, 0x02, 0x03 };

                _dapper.Update(d1);
                string sql = string.Format("select * from dapper_all_test where id = {0}", d1.Id);
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
                ClearTestEntity();
            }
        }
        [Fact(DisplayName = "MappingStrategy:异步更新操作")]
        public void UpdateSync()
        {
            ClearTestEntity();
            var d1 = CreateTestEntity();

            try
            {
                d1.BigintValue = 1;
                d1.BitValue = true;
                d1.BoolValue = true;
                d1.BooleanValue = true;
                d1.DateValue = DateTime.Now.AddDays(1);
                d1.CharNullValue = null;
                d1.DecValue = 8888;
                d1.DecimalValue = 8888;
                d1.DoubleValue = 888;
                d1.FixValue = 8881;
                d1.FloatValue = 888;
                d1.IntValue = 888;
                d1.IntegerValue = 8888;
                d1.LongtextNullValue = null;
                d1.MediumintValue = 8888;
                d1.MediumintNullValue = null;
                d1.NcharNullValue = null;
                d1.NvarcharNullValue = null;
                d1.NumericValue = 8888;
                d1.RealValue = 8888;
                d1.SmallintValue = 8888;
                d1.TextNullValue = "";
                d1.TinytextNullValue = null;
                d1.TinyintValue = 127;
                d1.VarbinaryNullValue = new byte[] { 0x01, 0x02, 0x03 };
                d1.BinaryNullValue = new byte[] { 0x01, 0x02, 0x03 };


                _dapper.UpdateAsync(d1).GetAwaiter().GetResult();
                string sql = string.Format("select * from dapper_all_test where id = {0}", d1.Id);
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
                ClearTestEntity();
            }
        }

        public void Dispose()
        {
            _testContext?.Dispose();
        }
    }
}
