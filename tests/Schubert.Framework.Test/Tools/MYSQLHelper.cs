using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.IO;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Schubert.Framework.Test.Tools
{
    class MySQLHelper
    {
        private readonly IDictionary<string, string> createTableSQLs = new Dictionary<string, string>();
        private readonly string _connectionString;
        public string ConnectionString
        {
            get { return _connectionString; }
        }
        public MySQLHelper(string connectionString)
        {
            this._connectionString = connectionString;
        }
        private T GetConnection<T>(Func<MySqlConnection, T> func)
        {
            using (MySqlConnection conn = new MySqlConnection(this._connectionString))
            {
                conn.Open();
                return func(conn);
            }
        }

        public T ExecuteSQL<T>(string sql, Func<MySqlCommand, T> func, bool throwException = true)
        {
            try
            {
                return GetConnection(conn =>
                 {
                     using (var command = new MySqlCommand(sql, conn))
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
        public int ExecuteSQL(string sql, bool throwException = true) => ExecuteSQL(sql, cmd => cmd.ExecuteNonQuery(), throwException);
        public void ExecuteReader(string sql, Action<DbDataReader> action)
        {
            GetConnection(conn =>
            {
                using (var command = new MySqlCommand(sql, conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        action(reader);
                    }
                }
                return 0;
            });
        }
        public T GetValue<T>(string sql, Func<object, T> converter) => converter(ExecuteSQL(sql, cmd => cmd.ExecuteScalar()));
        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connString"></param>
        /// <returns></returns>
        private bool IsTableExist(string tableName)
        {
            string sql = string.Format(@"select count(*) from information_schema.`TABLES` where TABLE_SCHEMA='test' and TABLE_NAME = '{0}'", tableName);
            return ExecuteSQL(sql, command =>
            {
                var result = command.ExecuteScalar();
                var count = 0;
                int.TryParse(result.ToString(), out count);
                return count >= 1;
            });
        }
        /// <summary>
        /// 数据库是否存在
        /// </summary>
        /// <param name="DataBaseName"></param>
        /// <param name="connString"></param>
        /// <returns></returns>
        private bool IsDataBaseExist(string databaseName)
        {
            string sql = string.Format(@"SELECT count(*) FROM information_schema.SCHEMATA where SCHEMA_NAME='{0}';", databaseName);
            return ExecuteSQL(sql, command =>
            {
                var result = command.ExecuteScalar();
                var count = 0;
                int.TryParse(result.ToString(), out count);
                return count >= 1;
            });
        }
        /// <summary>
        /// 如果数据库不存在，则创建
        /// </summary>
        public void CreateDataBaseIfNoExist(string databaseName)
        {
            if (IsDataBaseExist(databaseName)) return;
            ExecuteSQL($"create database if not exists @{databaseName}", command => command.ExecuteNonQuery());
        }
        /// <summary>
        /// 如果数据库中不存在表，则创建
        /// </summary>
        public void CreateTableIfNoExist(IList<string> tables)
        {
            foreach (string table in tables)
            {
                if (IsTableExist(table)) continue;
                var sql = "";
                if (!createTableSQLs.TryGetValue(table, out sql)) return;
                ExecuteSQL(sql, command => command.ExecuteNonQuery());
            }
        }
        /// <summary>
        /// 注册建表的sql
        /// </summary>
        /// <param name="sql">create sql</param>
        public void RegisteCreateTableSQL(string sql)
        {
            var regex = new Regex("(CREATE|create) *(TABLE|table) *`([A-Za-z0-9_]*)`");
            var tableName = regex.Match(sql).Groups[3].Value;
            createTableSQLs[tableName] = sql;
        }
    }
}