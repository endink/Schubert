using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Test.DapperIdentity
{
    public class DBSql
    {
        public static readonly string CreteDataBase = "create database if not exists {0}";

        public static string UserTokens = @"create table `UserTokens` (
                        `Id` bigint NOT NULL primary key,
                        `UserId` bigint NOT NULL,
                        `LoginProvider` varchar(16) NOT NULL,
                        `Name` varchar(32) NOT NULL,
                        `Value` varchar(128) NULL,
	                    constraint UK_UserId_LoginProvider_Name unique(`UserId`,`LoginProvider`,`Name`));";
        public static string UseLogins = @"create table `UseLogins` (
	                `Id` bigint NOT NULL primary key,
                    `LoginProvider`       varchar(16) NOT NULL,
                    `ProviderKey`         varchar(64) NOT NULL,
                    `ProviderDisplayName` varchar(16) NOT NULL,
                    `UserId`              bigint            NOT NULL,
	                constraint UK_LoginProvider_ProviderKey unique(`LoginProvider`,`ProviderKey`));";
        public static string UserClaims = @"create table `UserClaims` (
                    `Id`         bigint   NOT NULL primary key,
                    `ClaimType`  varchar (32) NULL,
                    `ClaimValue` varchar (64) NULL,
                    `UserId`     bigint            NOT NULL);";
        public static string UserBases = @"create table`UserBases` (
                    `Id`                   bigint         NOT NULL primary key,
                    `AccessFailedCount`    INT                NOT NULL,
                    `Email`                varchar (64)     NULL,
                    `EmailConfirmed`       bit                NOT NULL,
                    `Language`             varchar(32)     NOT NULL,
                    `LockoutEnabled`       bit                NOT NULL,
                    `LockoutEnd`           datetime NULL,
                    `NormalizedUserName`   varchar(16)    NOT NULL,
                    `PasswordHash`         varchar(256)     NOT NULL,
                    `PhoneNumber`          varchar(18)     NULL unique,
                    `PhoneNumberConfirmed` bit                NOT NULL,
                    `SecurityStamp`        varchar(32)     NULL,
                    `TimeZone`             varchar(32)     NULL,
                    `UserName`             varchar(16)     NOT NULL,
	                constraint UK_UserName unique(`UserName`),
	                constraint UK_PhoneNumber unique(`PhoneNumber`),
	                constraint UK_Email unique(`Email`));";
        public static string RoleBases = @"create table `RoleBases` (
                    `Id`         bigint   NOT NULL primary key,
                    `RoleName`   varchar (100) NOT NULL,
                    `DisplayName`  varchar (100)   NULL)
                     CHARACTER SET utf8
                     COLLATE utf8_general_ci
                    ;";
        public static string UserRoles = @"CREATE TABLE `userroles` (
                          `UserId` bigint(20) NOT NULL,
                          `RoleId` bigint(20) NOT NULL,
                          `RoleName` varchar(255) DEFAULT NULL,
                          PRIMARY KEY (`UserId`,`RoleId`));";

        public static string RoleClaim = @"CREATE TABLE `RoleClaims` (
                          `Id` bigint NOT NULL primary key,
                          `RoleId` bigint(20) NOT NULL,
                          `ClaimType` varchar (32) NULL,
                          `ClaimValue` varchar (64) NULL)
                           CHARACTER SET utf8
                           COLLATE utf8_general_ci
                            ;";
        
        public static Dictionary<string, string> ds = new Dictionary<string, string>();
        static DBSql()
        {
            ds.Add("UserTokens", UserTokens);
            ds.Add("UseLogins", UseLogins);
            ds.Add("UserClaims", UserClaims);
            ds.Add("UserBases", UserBases);
            ds.Add("RoleBases", RoleBases);
            ds.Add("UserRoles", UserRoles);
            ds.Add("RoleClaims", RoleClaim);
        }
        
        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connString"></param>
        /// <returns></returns>
        private static bool IsTableExist(string tableName, string connString)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                string sql = string.Format(@"select count(*) from information_schema.`TABLES` where TABLE_SCHEMA='test' and TABLE_NAME = '{0}'", tableName);
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                object result = cmd.ExecuteScalar();
                int count = 0;
                Int32.TryParse(result.ToString(), out count);
                if (count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 数据库是否存在
        /// </summary>
        /// <param name="DataBaseName"></param>
        /// <param name="connString"></param>
        /// <returns></returns>
        private static bool IsDataBaseExist(string DataBaseName, string connString)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                string sql = string.Format(@"SELECT count(*) FROM information_schema.SCHEMATA where SCHEMA_NAME='{0}';", DataBaseName);
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                object result = cmd.ExecuteScalar();
                int count = 0;
                Int32.TryParse(result.ToString(), out count);
                if (count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 如果数据库不存在，则创建
        /// </summary>
        public static void CreateDataBaseIfNoExist(string DataBaeName, string connString)
        {
            bool isExist = IsDataBaseExist(DataBaeName, connString);
            if (!isExist)  // 不存在
            {
                CreateDataBase(DataBaeName, connString);
            }
        }

        /// <summary>
        /// 如果数据库中不存在表，则创建
        /// </summary>
        public static void CreateTableIfNoExist(List<String> sqls, string connString)
        {
            foreach (string sql in sqls)
            {
                bool isExist = IsTableExist(sql, connString);
                if (!isExist)  // 不存在
                {
                    CreateTable(connString, sql);
                }
            }
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        private static void CreateDataBase(string DataBaseName, string connString)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                string mySelectQueryDataBase = string.Format(DBSql.CreteDataBase, DataBaseName);
                MySqlCommand cmdDataBase = new MySqlCommand(CreteDataBase, conn);
                cmdDataBase.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 创建表结构
        /// </summary>
        private static void CreateTable(string connString, string sql)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(ds[sql], conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
