using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Schubert.Framework.Data;
using Schubert.Framework.Data.Dapper.SQLite;
using Schubert.Framework.Data.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Schubert.Framework.Data.Conventions;
using Xunit;

namespace Schubert.Framework.Test.Dapper
{
    [Collection("dapper")]
    public class MultiDBContextConventionTest : IDisposable
    {
        private readonly MySQLHelper helper;
        private static readonly string MYSQL = "mysql";
        private static readonly string SQLITE = "sqlite";
        private static readonly string MySQLConnection = MySqlConnectionString.Value;
        private static readonly string SQLiteConnection =
                $"Data Source={System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("temp"), $"{DateTime.Now:yyyyMMddhhmmss}.db3")}";



        private readonly DapperContext _testContext;
        private readonly DapperRepository<DapperTestEntity> _mySQLRepository;
        private readonly DapperRepository<DapperTest> _sqliteRepository;

        public MultiDBContextConventionTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
            var options = new DapperDatabaseOptions
            {
                DefaultConnectionName = SQLITE,
                DefaultDatabaseProvider = typeof(SQLiteDatabaseProvider),
                ConnectionStrings = new Dictionary<string, string>
                {
                    [MYSQL] = MySQLConnection,
                    [SQLITE] = SQLiteConnection
                }
            };
            new DapperDataFeatureBuilder(options)
               .MapDatabaseProvider(SQLITE, typeof(SQLiteDatabaseProvider))
               .MapDatabaseProvider(MYSQL, typeof(MySqlDatabaseProvider))
               .Conventions(configure =>
                {
                    configure.Types(x => x == typeof(DapperTest))
                        .IsEntity(SQLITE);
                    configure.IsKey<DapperTest>(p => p.id);
                })
               .Build();
            var optionsMock = new Mock<IOptions<DapperDatabaseOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);
            _testContext = new DapperContext(new DapperRuntime(optionsMock.Object,
                    new IDapperMetadataProvider[] { new MySQLDapperMetadataProviderInstance() })
                , new LoggerFactory());
            _mySQLRepository = new DapperRepository<DapperTestEntity>(_testContext);
            Assert.NotNull(_mySQLRepository);
            _sqliteRepository = new DapperRepository<DapperTest>(_testContext);
            Assert.NotNull(_sqliteRepository);

            helper = MySQLHelper.Default;
            helper.CreateDataBaseIfNoExist("test");
            helper.CreateTableIfNoExist(new[] { "dapper_all_test1", "time_test" });
            SQLiteHelper.CreateTableIfNoExist("dapper_test");
            SQLiteHelper.CreateTableIfNoExist("test_table");
        }

        private void ClearMySQLDapperTestEntity()
            => helper.ExecuteSQL("delete from dapper_all_test", command => command.ExecuteNonQuery());
        private void ClearSQLiteDapperTestEntity()
            => SQLiteHelper.ExecuteSQL(@"delete from ""dapper_test""", command => command.ExecuteNonQuery());

        private DapperTestEntity CreateMySQLDapperTestEntity(int id = 1)
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
            var sql = $@"
                        insert into dapper_all_test(
                        bigint_value,bit_value,bool_value,boolean_value,date_value,dec_value,decimal_value,double_value,fix_value,float_value,int_value,integer_value,
                        mediumint_value,numeric_value,real_value,smallint_value,text_null_value,tinyint_value,id,datetime_value) 
                        value ({entity.bigint_value},{(entity.bit_value == true ? 1 : 0)},{
                    (entity.bool_value == true ? 1 : 0)
                },{(entity.boolean_value == true ? 1 : 0)},'{entity.date_value:yyyy-MM-dd HH:mm:ss}','{
                    entity.dec_value
                }',{entity.decimal_value},{entity.double_value},{entity.fix_value},{entity.float_value},{
                    entity.int_value
                },{
                    entity.integer_value
                },{entity.mediumint_value},{entity.numeric_value},{entity.real_value},{entity.smallint_value},'{
                    entity.text_null_value
                }',{entity.tinyint_value},{entity.id},'{entity.datetime_value:yyyy-MM-dd HH:mm:ss}')";
            if (helper.ExecuteSQL(sql, cmd => cmd.ExecuteNonQuery()) > 0) return entity;
            return null;
        }
        private DapperTest CreateSQLiteDapperTestEntity(int id = 1)
        {
            var entity = new DapperTest
            {
                id = id,
                bigint_value = 2,
                datetime_value = new DateTime(1988, 11, 12, 17, 34, 21),
                date_value = new DateTime(1988, 11, 12),
                decimal_value = 100m,
                dec_value = 200m,
                double_value = 300d,
                fix_value = 500.5m,
                float_value = 600.6f,
                integer_value = 7654321,
                int_value = 8888888,
                mediumint_value = 999,
                numeric_value = 10101,
                real_value = 12312,
                smallint_value = 23,
                tinyint_value = 11,
                longtext_null_value = "codemonk"
            };
            var sql = string.Format($@"
                        insert into ""dapper_test""(
""id"",
""bigint_value"",
""datetime_value"",
""date_value"",
""decimal_value"",
""dec_value"",
""double_value"",
""fix_value"",
""float_value"",
""integer_value"",
""int_value"",
""mediumint_value"",
""numeric_value"",
""real_value"",
""smallint_value"",
""tinyint_value"", 
""longtext_null_value"") values (
{entity.id},{entity.bigint_value},'{entity.datetime_value}','{entity.datetime_value}',
{entity.decimal_value},{entity.dec_value},{entity.double_value},{entity.fix_value},{entity.float_value},{entity.integer_value}
,{entity.int_value},{entity.mediumint_value},{entity.numeric_value},{entity.real_value},{entity.smallint_value},{entity.tinyint_value},'{entity.longtext_null_value}')");
            if (SQLiteHelper.ExecuteSQL(sql, cmd => cmd.ExecuteNonQuery()) > 0) return entity;
            return null;
        }

        [Fact(DisplayName = "MultiDBContext.Convention:连接名对比")]
        private void ContrastName()
        {
            string getDataSourceConnectionName<T>(DapperRepository<T> repository) where T : class
            {
                var datasource = repository
                    .GetType()
                    .GetProperty("DataSource", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(repository);
                var name = typeof(DapperDataSource)
                    .GetProperty("ReadingConnectionName")
                    .GetValue(datasource);
                return name as string;
            }
            Assert.Equal(MYSQL, getDataSourceConnectionName(_mySQLRepository));
            Assert.Equal(SQLITE, getDataSourceConnectionName(_sqliteRepository));
        }

        [Fact(DisplayName = "MultiDBContext.Convention:查询首个或者默认值")]
        private void QueryFirstOrDefault()
        {
            {
                ClearMySQLDapperTestEntity();
                var d1 = CreateMySQLDapperTestEntity();
                try
                {
                    var filter = new SingleQueryFilter().AddEqual("id", d1.id);
                    var result = _mySQLRepository.QueryFirstOrDefault(filter);
                    var sql = $"select * from dapper_all_test where id={d1.id}";
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
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                var d1 = CreateSQLiteDapperTestEntity();
                try
                {
                    var filter = new SingleQueryFilter().AddEqual("id", d1.id);
                    var result = _sqliteRepository.QueryFirstOrDefault(filter);
                    var sql = $"select * from dapper_test where id={d1.id}";
                    SQLiteHelper.ExecuteReader(sql, reader =>
                    {
                        if (reader.Read())
                        {
                            CompareHelper.Compare(reader, result);
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }
        [Fact(DisplayName = "MultiDBContext.Convention:异步查询首个或者默认值")]
        private void QueryFirstOrDefaultAsync()
        {
            {
                ClearMySQLDapperTestEntity();
                var d1 = CreateMySQLDapperTestEntity();
                try
                {
                    var filter = new SingleQueryFilter().AddEqual("id", d1.id);
                    var result = _mySQLRepository.QueryFirstOrDefaultAsync(filter).GetAwaiter().GetResult();
                    var sql = $"select * from dapper_all_test where id={d1.id}";
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
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                var d1 = CreateSQLiteDapperTestEntity();
                try
                {
                    var filter = new SingleQueryFilter().AddEqual("id", d1.id);
                    var result = _sqliteRepository.QueryFirstOrDefaultAsync(filter).GetAwaiter().GetResult();
                    var sql = $@"select * from ""dapper_test"" where ""id""={d1.id}";
                    SQLiteHelper.ExecuteReader(sql, reader =>
                    {
                        if (reader.Read())
                        {
                            CompareHelper.Compare(reader, result);
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }

        [Fact(DisplayName = "MultiDBContext.Convention:查询所有操作")]
        private void QueryAll()
        {
            {
                ClearMySQLDapperTestEntity();
                CreateMySQLDapperTestEntity();
                CreateMySQLDapperTestEntity(2);
                CreateMySQLDapperTestEntity(3);
                try
                {
                    _mySQLRepository.QueryFirstOrDefault(m => m.char_null_value == "x" || m.varchar_null_value == "y");
                    var result = _mySQLRepository.QueryAll() ?? new List<DapperTestEntity>();
                    var sql = "select * from dapper_all_test";
                    helper.ExecuteReader(sql, reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                        }
                    });
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                CreateSQLiteDapperTestEntity();
                CreateSQLiteDapperTestEntity(2);
                CreateSQLiteDapperTestEntity(3);
                try
                {
                    _sqliteRepository.QueryFirstOrDefault(m => m.char_null_value == "x" || m.varchar_null_value == "y");
                    var result = _sqliteRepository.QueryAll() ?? new List<DapperTest>();
                    var sql = "select * from dapper_test";
                    SQLiteHelper.ExecuteReader(sql, reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }
        [Fact(DisplayName = "MultiDBContext.Convention:异步查询所有操作")]
        private void QueryAllAsync()
        {
            {
                ClearMySQLDapperTestEntity();
                CreateMySQLDapperTestEntity();
                CreateMySQLDapperTestEntity(2);
                CreateMySQLDapperTestEntity(3);
                try
                {
                    var result = _mySQLRepository.QueryAllAsync().GetAwaiter().GetResult() ?? new List<DapperTestEntity>();
                    var sql = "select * from dapper_all_test";
                    helper.ExecuteReader(sql, reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                        }
                    });
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                CreateSQLiteDapperTestEntity();
                CreateSQLiteDapperTestEntity(2);
                CreateSQLiteDapperTestEntity(3);
                try
                {
                    var result = _sqliteRepository.QueryAllAsync().GetAwaiter().GetResult() ?? new List<DapperTest>();
                    var sql = "select * from dapper_test";
                    SQLiteHelper.ExecuteReader(sql, reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }

        [Fact(DisplayName = "MultiDBContext.Convention:查询操作")]
        private void Query()
        {
            {
                ClearMySQLDapperTestEntity();
                CreateMySQLDapperTestEntity();
                CreateMySQLDapperTestEntity(2);
                CreateMySQLDapperTestEntity(3);
                try
                {
                    var filter = new SingleQueryFilter().AddEqual(nameof(DapperTestEntity.dec_value), 1);
                    var result = _mySQLRepository.Query(filter);
                    helper.ExecuteReader("select * from dapper_all_test where id=1", reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                        }
                    });
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                CreateSQLiteDapperTestEntity();
                CreateSQLiteDapperTestEntity(2);
                CreateSQLiteDapperTestEntity(3);
                try
                {
                    var filter = new SingleQueryFilter().AddEqual(nameof(DapperTest.id), 2);
                    var result = _sqliteRepository.Query(filter);
                    SQLiteHelper.ExecuteReader(@"select * from ""dapper_test"" where ""id""=2", reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }
        [Fact(DisplayName = "MultiDBContext.Convention:异步查询操作")]
        private void QueryAsync()
        {
            {
                ClearMySQLDapperTestEntity();
                CreateMySQLDapperTestEntity();
                CreateMySQLDapperTestEntity(2);
                CreateMySQLDapperTestEntity(3);
                try
                {
                    var filter = new SingleQueryFilter().AddEqual(nameof(DapperTestEntity.dec_value), 1);
                    var result = _mySQLRepository.QueryAsync(filter).GetAwaiter().GetResult();
                    helper.ExecuteReader("select * from dapper_all_test where id=1", reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                        }
                    });
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                CreateSQLiteDapperTestEntity();
                CreateSQLiteDapperTestEntity(2);
                CreateSQLiteDapperTestEntity(3);
                try
                {
                    var filter = new SingleQueryFilter().AddEqual(nameof(DapperTest.id), 2);
                    var result = _sqliteRepository.QueryAsync(filter).GetAwaiter().GetResult();
                    SQLiteHelper.ExecuteReader("select * from dapper_test where id=2", reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }

        [Fact(DisplayName = "MultiDBContext.Convention:插入操作")]
        public void Insert()
        {
            {
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
                    ClearMySQLDapperTestEntity();

                    //Dapper插入对象
                    _mySQLRepository.Insert(entity);
                    var sql = $"select * from dapper_all_test where id = '{entity.id}'";
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
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                #region 实体对象赋值
                var entity = new DapperTest
                {
                    id = 22,
                    bigint_value = 2,
                    datetime_value = new DateTime(1988, 11, 12, 17, 34, 21),
                    date_value = new DateTime(1988, 11, 12),
                    decimal_value = 100m,
                    dec_value = 20.3m,
                    double_value = 300d,
                    fix_value = 500.5m,
                    float_value = 610f,
                    integer_value = 76543,
                    int_value = 888,
                    mediumint_value = 999,
                    numeric_value = 10101,
                    real_value = 12312,
                    smallint_value = 23,
                    tinyint_value = 11,
                    longtext_null_value = "codemonk"
                };
                #endregion

                try
                {
                    ClearSQLiteDapperTestEntity();

                    var result = SQLiteHelper.GetConnection(x => x.Execute(@"insert into ""test_table""(""CREATE_TIME"")values(:ct)",
                        new { ct = DateTime.UtcNow }));

                    Assert.True(result > 0);

                    _sqliteRepository.Insert(entity);
                    var sql = $@"select * from ""dapper_test"" where ""id"" = '{entity.id}'";
                    SQLiteHelper.ExecuteReader(sql, reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, entity);
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }
        [Fact(DisplayName = "MultiDBContext.Convention:异步插入操作")]
        public void InsertAsync()
        {
            {
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
                    ClearMySQLDapperTestEntity();

                    //Dapper插入对象
                    _mySQLRepository.InsertAsync(entity).GetAwaiter().GetResult();
                    var sql = $"select * from dapper_all_test where id = '{entity.id}'";
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
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                #region 实体对象赋值
                var entity = new DapperTest
                {
                    id = 22,
                    bigint_value = 2,
                    datetime_value = new DateTime(1988, 11, 12, 17, 34, 21),
                    date_value = new DateTime(1988, 11, 12),
                    decimal_value = 100m,
                    dec_value = 20.3m,
                    double_value = 300d,
                    fix_value = 500.5m,
                    float_value = 610f,
                    integer_value = 76543,
                    int_value = 888,
                    mediumint_value = 999,
                    numeric_value = 10101,
                    real_value = 12312,
                    smallint_value = 23,
                    tinyint_value = 11,
                    longtext_null_value = "codemonk"
                };
                #endregion

                try
                {
                    ClearSQLiteDapperTestEntity();

                    var result = SQLiteHelper.GetConnection(x => x.Execute(@"insert into ""test_table""(""CREATE_TIME"")values(:ct)",
                        new { ct = DateTime.UtcNow }));

                    Assert.True(result > 0);

                    _sqliteRepository.InsertAsync(entity).GetAwaiter().GetResult();
                    var sql = $@"select * from ""dapper_test"" where ""id"" = '{entity.id}'";
                    SQLiteHelper.ExecuteReader(sql, reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, entity);
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }

        [Fact(DisplayName = "MultiDBContext.Convention:批量插入操作")]
        public void InsertAll()
        {
            {
                DapperTestEntity Build(int x)
                {
                    return new DapperTestEntity()
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
                    };
                }

                try
                {
                    ClearMySQLDapperTestEntity();

                    //Dapper插入对象
                    var count = _mySQLRepository.Insert(new[]
                    {
                        Build(1),
                        Build(2),
                        Build(3),
                        Build(4),
                        Build(5)
                    });
                    Assert.Equal(count, 5);
                    var value = helper.GetValue("select count(1) from dapper_all_test", Convert.ToInt32);
                    Assert.Equal(value, 5);
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {

                DapperTest Build(int x)
                {
                    return new DapperTest
                    {
                        id = x,
                        bigint_value = 2,
                        datetime_value = new DateTime(1988, 11, 12, 17, 34, 21),
                        date_value = new DateTime(1988, 11, 12),
                        decimal_value = 100m,
                        dec_value = 20.3m,
                        double_value = 300d,
                        fix_value = 500.5m,
                        float_value = 610f,
                        integer_value = 76543,
                        int_value = 888,
                        mediumint_value = 999,
                        numeric_value = 10101,
                        real_value = 12312,
                        smallint_value = 23,
                        tinyint_value = 11,
                        longtext_null_value = "codemonk"
                    };
                }
                try
                {
                    ClearMySQLDapperTestEntity();
                    var count = _sqliteRepository.Insert(new[] {
                        Build(1),
                        Build(2),
                        Build(3),
                        Build(4),
                        Build(5)
                    });
                    Assert.Equal(count, 5);
                    var value = SQLiteHelper.GetValue("select count(1) from dapper_test", Convert.ToInt32);
                    Assert.Equal(value, 5);
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
        }
        [Fact(DisplayName = "MultiDBContext.Convention:批量插入操作")]
        public void InsertAllAsync()
        {
            {
                DapperTestEntity Build(int x)
                {
                    return new DapperTestEntity()
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
                    };
                }

                try
                {
                    ClearMySQLDapperTestEntity();

                    //Dapper插入对象
                    var count = _mySQLRepository.InsertAsync(new[]
                    {
                        Build(1),
                        Build(2),
                        Build(3),
                        Build(4),
                        Build(5)
                    }).GetAwaiter().GetResult();
                    Assert.Equal(count, 5);
                    var value = helper.GetValue("select count(1) from dapper_all_test", Convert.ToInt32);
                    Assert.Equal(value, 5);
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {

                DapperTest Build(int x)
                {
                    return new DapperTest
                    {
                        id = x,
                        bigint_value = 2,
                        datetime_value = new DateTime(1988, 11, 12, 17, 34, 21),
                        date_value = new DateTime(1988, 11, 12),
                        decimal_value = 100m,
                        dec_value = 20.3m,
                        double_value = 300d,
                        fix_value = 500.5m,
                        float_value = 610f,
                        integer_value = 76543,
                        int_value = 888,
                        mediumint_value = 999,
                        numeric_value = 10101,
                        real_value = 12312,
                        smallint_value = 23,
                        tinyint_value = 11,
                        longtext_null_value = "codemonk"
                    };
                }
                try
                {
                    ClearSQLiteDapperTestEntity();
                    var count = _sqliteRepository.InsertAsync(new[] {
                        Build(1),
                        Build(2),
                        Build(3),
                        Build(4),
                        Build(5)
                    }).GetAwaiter().GetResult();
                    Assert.Equal(count, 5);
                    var value = SQLiteHelper.GetValue("select count(1) from dapper_test", Convert.ToInt32);
                    Assert.Equal(value, 5);
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }

        [Fact(DisplayName = "MultiDBContext.Convention:删除操作")]
        public void Delete()
        {
            {
                ClearMySQLDapperTestEntity();
                try
                {
                    var d1 = CreateMySQLDapperTestEntity();
                    _mySQLRepository.Delete(new DapperTestEntity { id = d1.id });
                    var sql = $"select count(*) from dapper_all_test where id = {d1.id}";
                    var count = helper.GetValue(sql, Convert.ToInt32);
                    Assert.Equal(count, 0);
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                try
                {
                    var d1 = CreateSQLiteDapperTestEntity();
                    _sqliteRepository.Delete(new DapperTest { id = d1.id });
                    var sql = $"select count(*) from dapper_test where id = {d1.id}";
                    var count = SQLiteHelper.GetValue(sql, Convert.ToInt32);
                    Assert.Equal(count, 0);
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }
        [Fact(DisplayName = "MultiDBContext.Convention:异步删除操作")]
        public void DeleteSync()
        {
            {
                ClearMySQLDapperTestEntity();
                try
                {
                    var d1 = CreateMySQLDapperTestEntity();
                    _mySQLRepository.DeleteAsync(new DapperTestEntity { id = d1.id }).GetAwaiter().GetResult();
                    var sql = $"select count(*) from dapper_all_test where id = {d1.id}";
                    var count = helper.GetValue(sql, Convert.ToInt32);
                    Assert.Equal(count, 0);
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                try
                {
                    var d1 = CreateSQLiteDapperTestEntity();
                    _sqliteRepository.DeleteAsync(new DapperTest { id = d1.id }).GetAwaiter().GetResult();
                    var sql = $"select count(*) from dapper_test where id = {d1.id}";
                    var count = SQLiteHelper.GetValue(sql, Convert.ToInt32);
                    Assert.Equal(count, 0);
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }

        [Fact(DisplayName = "MultiDBContext.Convention:更新操作")]
        public void Update()
        {
            {
                ClearMySQLDapperTestEntity();
                var d1 = CreateMySQLDapperTestEntity();

                try
                {
                    #region 实体对象赋值 
                    d1.id = 22;
                    d1.bigint_value = 2;
                    d1.datetime_value = new DateTime(1988, 11, 12, 17, 34, 21);
                    d1.date_value = new DateTime(1988, 11, 12);
                    d1.decimal_value = 100m;
                    d1.dec_value = 20.3m;
                    d1.double_value = 300d;
                    d1.fix_value = 500.5m;
                    d1.float_value = 600.6f;
                    d1.integer_value = 76543;
                    d1.int_value = 888;
                    d1.mediumint_value = 999;
                    d1.numeric_value = 10101;
                    d1.real_value = 12312;
                    d1.smallint_value = 23;
                    d1.tinyint_value = 11;
                    d1.longtext_null_value = "codemonk";
                    #endregion

                    _mySQLRepository.Update(d1);
                    var sql = $"select * from dapper_all_test where id = {d1.id}";
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
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                var d1 = CreateSQLiteDapperTestEntity();

                try
                {
                    #region 实体对象赋值 
                    d1.id = 22;
                    d1.bigint_value = 2;
                    d1.datetime_value = new DateTime(1988, 11, 12, 17, 34, 21);
                    d1.date_value = new DateTime(1988, 11, 12);
                    d1.decimal_value = 100m;
                    d1.dec_value = 20.3m;
                    d1.double_value = 300d;
                    d1.fix_value = 500.5m;
                    d1.float_value = 600.6f;
                    d1.integer_value = 76543;
                    d1.int_value = 888;
                    d1.mediumint_value = 999;
                    d1.numeric_value = 10101;
                    d1.real_value = 12312;
                    d1.smallint_value = 23;
                    d1.tinyint_value = 11;
                    d1.longtext_null_value = "codemonk";
                    #endregion

                    _sqliteRepository.Update(d1);
                    var sql = $@"select * from ""dapper_test"" where ""id""={d1.id}";
                    SQLiteHelper.ExecuteReader(sql, reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, d1);
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }
        [Fact(DisplayName = "MultiDBContext.Convention:异步更新操作")]
        public void UpdateAsync()
        {
            {
                ClearMySQLDapperTestEntity();
                var d1 = CreateMySQLDapperTestEntity();

                try
                {
                    #region 实体对象赋值 
                    d1.id = 22;
                    d1.bigint_value = 2;
                    d1.datetime_value = new DateTime(1988, 11, 12, 17, 34, 21);
                    d1.date_value = new DateTime(1988, 11, 12);
                    d1.decimal_value = 100m;
                    d1.dec_value = 20.3m;
                    d1.double_value = 300d;
                    d1.fix_value = 500.5m;
                    d1.float_value = 600.6f;
                    d1.integer_value = 76543;
                    d1.int_value = 888;
                    d1.mediumint_value = 999;
                    d1.numeric_value = 10101;
                    d1.real_value = 12312;
                    d1.smallint_value = 23;
                    d1.tinyint_value = 11;
                    d1.longtext_null_value = "codemonk";
                    #endregion

                    _mySQLRepository.UpdateAsync(d1).GetAwaiter().GetResult();
                    var sql = $"select * from dapper_all_test where id = {d1.id}";
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
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                var d1 = CreateSQLiteDapperTestEntity();

                try
                {
                    #region 实体对象赋值 
                    d1.id = 22;
                    d1.bigint_value = 2;
                    d1.datetime_value = new DateTime(1988, 11, 12, 17, 34, 21);
                    d1.date_value = new DateTime(1988, 11, 12);
                    d1.decimal_value = 100m;
                    d1.dec_value = 20.3m;
                    d1.double_value = 300d;
                    d1.fix_value = 500.5m;
                    d1.float_value = 600.6f;
                    d1.integer_value = 76543;
                    d1.int_value = 888;
                    d1.mediumint_value = 999;
                    d1.numeric_value = 10101;
                    d1.real_value = 12312;
                    d1.smallint_value = 23;
                    d1.tinyint_value = 11;
                    d1.longtext_null_value = "codemonk";
                    #endregion

                    _sqliteRepository.UpdateAsync(d1).GetAwaiter().GetResult();
                    var sql = $@"select * from ""dapper_test"" where ""id""={d1.id}";
                    SQLiteHelper.ExecuteReader(sql, reader =>
                    {
                        while (reader.Read())
                        {
                            CompareHelper.Compare(reader, d1);
                        }
                    });
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }


        [Fact(DisplayName = "MultiDBContext.Convention:分页查询")]
        public void QueryPage()
        {
            {
                ClearMySQLDapperTestEntity();
                try
                {
                    const int length = 50;
                    for (var i = 0; i < length; i++)
                    {
                        CreateMySQLDapperTestEntity(i + 1);
                    }
                    var count = _mySQLRepository.QueryPage(2, 10).ToArray().Length;
                    Assert.Equal(count, 10);
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                try
                {
                    const int length = 50;
                    for (var i = 0; i < length; i++)
                    {
                        CreateSQLiteDapperTestEntity(i + 1);
                    }
                    var count = _sqliteRepository.QueryPage(2, 10).ToArray().Length;
                    Assert.Equal(count, 10);
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }
        [Fact(DisplayName = "MultiDBContext.Convention:异步分页查询")]
        public void QueryPageAsync()
        {
            {
                ClearMySQLDapperTestEntity();
                try
                {
                    const int length = 50;
                    for (var i = 0; i < length; i++)
                    {
                        CreateMySQLDapperTestEntity(i + 1);
                    }
                    var count = _mySQLRepository.QueryPageAsync(2, 10).GetAwaiter().GetResult().ToArray().Length;
                    Assert.Equal(count, 10);
                }
                finally
                {
                    ClearMySQLDapperTestEntity();
                }
            }
            {
                ClearSQLiteDapperTestEntity();
                try
                {
                    const int length = 50;
                    for (var i = 0; i < length; i++)
                    {
                        CreateSQLiteDapperTestEntity(i + 1);
                    }
                    var count = _sqliteRepository.QueryPageAsync(2, 10).GetAwaiter().GetResult().ToArray().Length;
                    Assert.Equal(count, 10);
                }
                finally
                {
                    ClearSQLiteDapperTestEntity();
                }
            }
        }

        public void Dispose()
        {
            _testContext?.Dispose();
        }

        private static class SQLiteHelper
        {
            private static readonly IDictionary<string, string> createTableSQLs = new Dictionary<string, string>();
            private static void RegisteCreateTableSQL(string sql)
            {
                var regex = new Regex(@"(CREATE|create) *(TABLE|table) *""([A-Za-z0-9_]*)""");
                var tableName = regex.Match(sql).Groups[3].Value;
                createTableSQLs[tableName] = sql;
            }
            static SQLiteHelper()
            {
                RegisteCreateTableSQL(@"CREATE TABLE ""dapper_test"" (
                          ""id"" interger   default null,
                          ""bigint_value"" interger  NOT NULL,
                          ""bigint_null_value"" varchar(255) DEFAULT NULL, 
                          ""char_null_value"" varchar(255) DEFAULT NULL,
                          ""date_value"" date DEFAULT NULL,
                          ""date_null_value"" date DEFAULT NULL,
                          ""datetime_value"" date  NOT NULL,
                          ""datetime_null_value"" date DEFAULT NULL,
                          ""dec_value"" double NOT NULL,
                          ""dec_null_value"" varchar(255) DEFAULT NULL,
                          ""decimal_value"" decimal(10, 2) NOT NULL,
                          ""decimal_null_value""varchar(255) DEFAULT NULL,
                          ""double_value"" double NOT NULL,
                          ""double_null_value"" varchar(255) DEFAULT NULL,
                          ""fix_value"" decimal(12, 2) NOT NULL,
                          ""fix_null_value"" varchar(255) DEFAULT NULL,
                          ""float_value"" varchar(255) NOT NULL,
                          ""float_null_value"" varchar(255) DEFAULT NULL,
                          ""int_value"" interger  NOT NULL,
                          ""int_null_value"" varchar(255) DEFAULT NULL,
                          ""integer_value"" interger  NOT NULL,
                          ""integer_null_value"" varchar(255) DEFAULT NULL,
                          ""linestring_null_value"" nvarchar2(255) DEFAULT NULL,
                          ""longtext_null_value"" text DEFAULT NULL,
                          ""mediumint_value"" decimal(9) NOT NULL,
                          ""mediumint_null_value"" decimal(9) DEFAULT NULL,
                          ""mediumtext_null_value"" varchar(255) DEFAULT NULL,
                          ""nchar_null_value"" char(255) DEFAULT NULL,
                          ""numeric_value"" decimal(10, 0) NOT NULL,
                          ""numeric_null_value"" decimal(10, 0) DEFAULT NULL,
                          ""nvarchar_null_value"" varchar(255) DEFAULT NULL,
                          ""real_value"" decimal(15, 5) NOT NULL,
                          ""real_null_value"" varchar(255) DEFAULT NULL,
                          ""smallint_value"" decimal(6) NOT NULL,
                          ""smallint_null_value"" varchar(255) DEFAULT NULL,
                          ""text_null_value"" text, 
                          ""tinyint_value"" decimal(4) NOT NULL,
                          ""tinyint_null_value"" varchar(255) DEFAULT NULL,
                          ""tinytext_null_value"" varchar(255) DEFAULT NULL,
                          ""varbinary_null_value"" text  DEFAULT NULL,
                          ""varchar_null_value"" varchar(255) DEFAULT NULL,
                          ""binary_null_value"" text DEFAULT NULL
                        )");
                RegisteCreateTableSQL(@"CREATE TABLE ""test_table"" (CREATE_TIME date not null)");
            }
            [System.Diagnostics.DebuggerHidden]
            public static T GetConnection<T>(Func<SqliteConnection, T> func)
            {
                using (var conn = new SqliteConnection(SQLiteConnection))
                {
                    conn.Open();
                    return func(conn);
                }
            }
            [System.Diagnostics.DebuggerHidden]
            public static T ExecuteSQL<T>(string sql, Func<SqliteCommand, T> func, bool throwException = true)
            {
                try
                {
                    return GetConnection(conn =>
                    {
                        using (var command = new SqliteCommand(sql, conn))
                        {
                            return func(command);
                        }
                    });
                }
                catch (Exception exception)
                {
                    if (throwException)
                    {
                        var realExcption = exception;
                        while (true)
                        {
                            if (realExcption.InnerException == null) break;
                            realExcption = realExcption.InnerException;
                        }
                        throw realExcption;
                    }
                    return default(T);
                }
            }
            [System.Diagnostics.DebuggerHidden]
            public static int ExecuteSQL(string sql, bool throwException = true) => ExecuteSQL(sql, cmd => cmd.ExecuteNonQuery(), throwException);
            [System.Diagnostics.DebuggerHidden]
            public static T GetValue<T>(string sql, Func<object, T> converter) => converter(ExecuteSQL(sql, cmd => cmd.ExecuteScalar()));
            [System.Diagnostics.DebuggerHidden]
            public static void ExecuteReader(string sql, Action<DbDataReader> action)
            {
                GetConnection(conn =>
                {
                    using (var command = new SqliteCommand(sql, conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            action(reader);
                        }
                    }
                    return 0;
                });
            }
            [System.Diagnostics.DebuggerHidden]
            private static bool IsTableExist(string tableName)
            {
                var sql = $@"select count(1) from sqlite_master where type=""table"" and  name=""{tableName}""";
                return ExecuteSQL(sql, command =>
                {
                    var result = command.ExecuteScalar();
                    if (result == null) return false;
                    int.TryParse(result.ToString(), out var count);
                    return count >= 1;
                });
            }
            [System.Diagnostics.DebuggerHidden]
            public static void CreateTableIfNoExist(params string[] tables)
            {
                foreach (var table in tables)
                {
                    if (IsTableExist(table)) continue;
                    if (!createTableSQLs.TryGetValue(table, out var sql)) return;
                    ExecuteSQL(sql, command => command.ExecuteNonQuery());
                }
            }
        }
        private class MySQLDapperMetadataProviderInstance : DapperMetadataProvider<DapperTestEntity>
        {
            protected override void ConfigureModel(DapperMetadataBuilder<DapperTestEntity> builder)
            {
                builder.HasKey(d => d.id).TableName("dapper_all_test").DataSource(MYSQL);
            }
        }
    }
}
