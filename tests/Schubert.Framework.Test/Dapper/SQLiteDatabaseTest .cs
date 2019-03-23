using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Schubert.Framework.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Schubert.Framework.Data.Dapper.SQLite;
using Xunit;

namespace Schubert.Framework.Test.Dapper
{
    [Collection("dapper")]
    public class SQLiteDatabaseTest : IDisposable
    {
        private static readonly string fileName =
            System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("temp"), $"{DateTime.Now:yyyyMMddhhmmss}.db3");
        private static readonly string ConnectionString = $"Data Source={fileName}";
        private readonly DapperRepository<SQLiteDapperTestEntity> _dapper;
        private DapperContext _testContext;
        private DapperRepository<SQLiteDapperTestEntity> MockGenerator()
        {
            var optionsMock = new Mock<IOptions<DapperDatabaseOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new DapperDatabaseOptions
            {
                DefaultConnectionName = "default",
                DefaultDatabaseProvider = typeof(SQLiteDatabaseProvider),
                ConnectionStrings = new Dictionary<string, string>
                {
                    { "default",ConnectionString}
                }
            });
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddDebug(LogLevel.Trace);
            var rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderInstance() });
            _testContext = new DapperContext(rt, loggerFactory);
            return new DapperRepository<SQLiteDapperTestEntity>(_testContext);
        }
        //SQLite存储float类型的话会有精度问题
        public SQLiteDatabaseTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
            SQLiteHelper.CreateTableIfNoExist("dapper_all_test");
            SQLiteHelper.CreateTableIfNoExist("test_table");
            _dapper = MockGenerator();
        }
        public void Dispose()
        {
            _testContext?.Dispose();
        }

        private void ClearDapperTestEntity()
        {
            SQLiteHelper.ExecuteSQL(@"delete from ""dapper_all_test""", command => command.ExecuteNonQuery());
        }
        private SQLiteDapperTestEntity CreateDapperTestEntity(int id = 1)
        {
            var entity = new SQLiteDapperTestEntity
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
                        insert into ""dapper_all_test""(
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
        [Fact(DisplayName = "DapperSQLite:查询首个或者默认值")]
        private void QueryFirstOrDefault()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual("id", d1.id);
                var result = dapperInstance.QueryFirstOrDefault(filter);
                var sql = $@"select * from ""dapper_all_test"" where ""id""={d1.id}";
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
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:异步查询首个或者默认值")]
        private void QueryFirstOrDefaultAsync()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();
            try
            {
                var filter = new SingleQueryFilter().AddEqual("id", d1.id);
                var result = _dapper.QueryFirstOrDefaultAsync(filter).GetAwaiter().GetResult();
                var sql = $@"select * from ""dapper_all_test"" where ""id""={d1.id}";
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
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:查询所有操作")]
        private void QueryAll()
        {
            ClearDapperTestEntity();
            CreateDapperTestEntity();
            CreateDapperTestEntity(2);
            CreateDapperTestEntity(3);
            try
            {
                var result = _dapper.QueryAll() ?? new List<SQLiteDapperTestEntity>();
                var sql = @"select * from ""dapper_all_test""";
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
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:异步查询所有操作")]
        private void QueryAllAsync()
        {
            ClearDapperTestEntity();
            CreateDapperTestEntity();
            CreateDapperTestEntity(2);
            CreateDapperTestEntity(3);
            try
            {
                var result = _dapper.QueryAllAsync().GetAwaiter().GetResult() ?? new List<SQLiteDapperTestEntity>();
                var sql = @"select * from ""dapper_all_test""";
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
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:查询操作")]
        private void Query()
        {
            ClearDapperTestEntity();
            CreateDapperTestEntity();
            CreateDapperTestEntity(2);
            CreateDapperTestEntity(3);
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual(nameof(DapperEntityWithNoBool.dec_value), 200m);
                var result = dapperInstance.Query(filter);
                SQLiteHelper.ExecuteReader(@"select * from ""dapper_all_test"" where ""id""=1", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:异步查询操作")]
        private void QueryAsync()
        {
            ClearDapperTestEntity();
            CreateDapperTestEntity();
            CreateDapperTestEntity(2);
            CreateDapperTestEntity(3);
            try
            {
                var dapperInstance = _dapper;
                var filter = new SingleQueryFilter().AddEqual(nameof(DapperEntityWithNoBool.dec_value), 200m);
                var result = dapperInstance.QueryAsync(filter).GetAwaiter().GetResult();
                SQLiteHelper.ExecuteReader(@"select * from ""dapper_all_test"" where ""id""=1", reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, result.FirstOrDefault(x => x.id == Convert.ToInt32(reader["id"])));
                    }
                });
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:插入操作")]
        public void Insert()
        {
            var dapperInstance = _dapper;

            #region 实体对象赋值
            var entity = new SQLiteDapperTestEntity
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
                ClearDapperTestEntity();

                var result = SQLiteHelper.GetConnection(x => x.Execute(@"insert into ""test_table""(""CREATE_TIME"")values(:ct)", new { ct = DateTime.UtcNow }));
                Assert.True(result > 0);


                //Dapper插入对象
                dapperInstance.Insert(entity);
                var sql = $@"select * from ""dapper_all_test"" where ""id"" = '{entity.id}'";
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
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:异步插入操作")]
        public void InsertSync()
        {
            var dapperInstance = _dapper;

            #region 实体对象赋值
            var entity = new SQLiteDapperTestEntity
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
                ClearDapperTestEntity();

                var result = SQLiteHelper.GetConnection(x => x.Execute(@"insert into ""test_table""(""CREATE_TIME"")values(:ct)", new { ct = DateTime.UtcNow }));
                Assert.True(result > 0);


                //Dapper插入对象
                dapperInstance.InsertAsync(entity).GetAwaiter().GetResult();
                var sql = $@"select * from ""dapper_all_test"" where ""id"" = '{entity.id}'";
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
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:批量插入操作")]
        public void InsertAll()
        {
            var dapperInstance = _dapper;

            SQLiteDapperTestEntity Build(int x)
            {
                return new SQLiteDapperTestEntity
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
                ClearDapperTestEntity();
                var count = dapperInstance.Insert(new[] {
                    Build(1),
                    Build(2),
                    Build(3),
                    Build(4),
                    Build(5)
                });
                Assert.Equal(count, 5);
                var value = SQLiteHelper.GetValue("select count(1) from dapper_all_test", Convert.ToInt32);
                Assert.Equal(value, 5);
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:删除操作")]
        public void Delete()
        {
            ClearDapperTestEntity();
            try
            {
                var d1 = CreateDapperTestEntity();
                _dapper.Delete(new SQLiteDapperTestEntity { id = d1.id });
                var sql = $@"select count(*) from ""dapper_all_test"" where ""id"" = {d1.id}";
                var count = SQLiteHelper.GetValue(sql, Convert.ToInt32);
                Assert.Equal(count, 0);
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:异步删除操作")]
        public void DeleteSync()
        {
            ClearDapperTestEntity();
            try
            {
                var d1 = CreateDapperTestEntity();
                _dapper.DeleteAsync(new SQLiteDapperTestEntity { id = d1.id }).GetAwaiter().GetResult();
                var sql = $@"select count(*) from ""dapper_all_test"" where ""id"" = {d1.id}";
                var count = SQLiteHelper.GetValue(sql, Convert.ToInt32);
                Assert.Equal(count, 0);
            }
            finally
            {
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:更新操作")]
        public void Update()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();

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

                _dapper.Update(d1);
                var sql = $@"select * from ""dapper_all_test"" where ""id""={d1.id}";
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
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:异步更新操作")]
        public void UpdateSync()
        {
            ClearDapperTestEntity();
            var d1 = CreateDapperTestEntity();

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

                _dapper.UpdateAsync(d1).GetAwaiter().GetResult();
                var sql = $@"select * from ""dapper_all_test"" where ""id""={d1.id}";
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
                ClearDapperTestEntity();
            }
        }
        [Fact(DisplayName = "DapperSQLite:分页查询")]
        public void QueryPage()
        {
            ClearDapperTestEntity();
            try
            {
                const int length = 50;
                for (var i = 0; i < length; i++)
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

        class DapperMetadataProviderInstance : DapperMetadataProvider<SQLiteDapperTestEntity>
        {
            protected override void ConfigureModel(DapperMetadataBuilder<SQLiteDapperTestEntity> builder)
            {
                builder.HasKey(d => d.id)
                    .TableName("dapper_all_test")
                    .DataSource("default");
            }
        }

        static class SQLiteHelper
        {
            private static readonly IDictionary<string, string> createTableSQLs = new Dictionary<string, string>();
            public static void RegisteCreateTableSQL(string sql)
            {
                var regex = new Regex(@"(CREATE|create) *(TABLE|table) *""([A-Za-z0-9_]*)""");
                var tableName = regex.Match(sql).Groups[3].Value;
                createTableSQLs[tableName] = sql;
            }
            static SQLiteHelper()
            {
                RegisteCreateTableSQL(@"CREATE TABLE ""dapper_all_test"" (
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
                using (var conn = new SqliteConnection(ConnectionString))
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
    }
}
