using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.RegularExpressions;
using Helper = Schubert.Framework.Test.Tools.MySQLHelper;
namespace Schubert.Framework.Test.Dapper
{
    class MySQLHelper : Helper
    {
        public static readonly MySQLHelper Default = new MySQLHelper(MySqlConnectionString.Value);
        static MySQLHelper()
        {
            Default.RegisteCreateTableSQL(@"CREATE TABLE `dapper_compisite_key` (
                          products_id int(11) UNSIGNED NOT NULL AUTO_INCREMENT,
                          language_id int(11) NOT NULL DEFAULT 1,
                          products_name varchar(64) NOT NULL DEFAULT '',
                          products_description text DEFAULT NULL,
                          products_short_description text DEFAULT NULL,
                          products_url varchar(255) DEFAULT NULL,
                          products_viewed int(5) DEFAULT 0,
                          PRIMARY KEY (products_id, language_id),
                          INDEX products_name (products_name)
                        )
                        ENGINE = INNODB
                        AUTO_INCREMENT = 5
                        AVG_ROW_LENGTH = 2730
                        CHARACTER SET utf8
                        COLLATE utf8_general_ci;");
            Default.RegisteCreateTableSQL(@"CREATE TABLE `dapper_all_test` (
                          `id` int(11) unsigned DEFAULT NULL,
                          `bigint_value` bigint(20) NOT NULL,
                          `bigint_null_value` bigint(20) DEFAULT NULL,
                          `bit_value` bit(1) NOT NULL,
                          `bit_null_value` bit(1) DEFAULT NULL,
                          `bool_value` tinyint(1) NOT NULL,
                          `bool_null_value` tinyint(1) DEFAULT NULL,
                          `boolean_value` tinyint(1) NOT NULL,
                          `boolean_null_value` tinyint(1) DEFAULT NULL,
                          `char_null_value` char(255) DEFAULT NULL,
                          `date_value` date NOT NULL,
                          `date_null_value` date DEFAULT NULL,
                          `datetime_value` datetime NOT NULL,
                          `datetime_null_value` datetime DEFAULT NULL,
                          `dec_value` decimal(10,0) NOT NULL,
                          `dec_null_value` decimal(10,0) DEFAULT NULL,
                          `decimal_value` decimal(10,0) NOT NULL,
                          `decimal_null_value` varchar(255) DEFAULT NULL,
                          `double_value` double NOT NULL,
                          `double_null_value` varchar(255) DEFAULT NULL,
                          `fix_value` decimal(10,0) NOT NULL,
                          `fix_null_value` decimal(10,0) DEFAULT NULL,
                          `float_value` float NOT NULL,
                          `float_null_value` float DEFAULT NULL,
                          `int_value` int(11) NOT NULL,
                          `int_null_value` int(11) DEFAULT NULL,
                          `integer_value` int(11) NOT NULL,
                          `integer_null_value` varchar(255) DEFAULT NULL,
                          `linestring_null_value` linestring DEFAULT NULL,
                          `longtext_null_value` varchar(255) DEFAULT NULL,
                          `mediumint_value` mediumint(9) NOT NULL,
                          `mediumint_null_value` mediumint(9) DEFAULT NULL,
                          `mediumtext_null_value` mediumtext,
                          `nchar_null_value` char(255) DEFAULT NULL,
                          `numeric_value` decimal(10,0) NOT NULL,
                          `numeric_null_value` decimal(10,0) DEFAULT NULL,
                          `nvarchar_null_value` varchar(255) DEFAULT NULL,
                          `real_value` double NOT NULL,
                          `real_null_value` double DEFAULT NULL,
                          `smallint_value` smallint(6) NOT NULL,
                          `smallint_null_value` smallint(6) DEFAULT NULL,
                          `text_null_value` text,
                          `time_null_value` time DEFAULT NULL,
                          `timestamp_value` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                          `timestamp_null_value` timestamp NULL DEFAULT '0000-00-00 00:00:00',
                          `tinyint_value` tinyint(4) NOT NULL,
                          `tinyint_null_value` tinyint(4) DEFAULT NULL,
                          `tinytext_null_value` tinytext,
                          `varbinary_null_value` varbinary(255) DEFAULT NULL,
                          `varchar_null_value` varchar(255) DEFAULT NULL,
                          `binary_null_value` binary(20) DEFAULT NULL
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8;");
            Default.RegisteCreateTableSQL(@"CREATE TABLE `DapperAllTest` (
                          `Id` int(11) unsigned DEFAULT NULL,
                          `BigintValue` bigint(20) NOT NULL,
                          `BigintNullValue` bigint(20) DEFAULT NULL,
                          `BitValue` bit(1) NOT NULL,
                          `BitNullValue` bit(1) DEFAULT NULL,
                          `BoolValue` tinyint(1) NOT NULL,
                          `BoolNullValue` tinyint(1) DEFAULT NULL,
                          `BooleanValue` tinyint(1) NOT NULL,
                          `BooleanNullValue` tinyint(1) DEFAULT NULL,
                          `CharNullValue` char(255) DEFAULT NULL,
                          `DateValue` date NOT NULL,
                          `DateNullValue` date DEFAULT NULL,
                          `DatetimeValue` datetime NOT NULL,
                          `DatetimeNullValue` datetime DEFAULT NULL,
                          `DecValue` decimal(10,0) NOT NULL,
                          `DecNullValue` decimal(10,0) DEFAULT NULL,
                          `DecimalValue` decimal(10,0) NOT NULL,
                          `DecimalNullValue` varchar(255) DEFAULT NULL,
                          `DoubleValue` double NOT NULL,
                          `DoubleNullValue` varchar(255) DEFAULT NULL,
                          `FixValue` decimal(10,0) NOT NULL,
                          `FixNullValue` decimal(10,0) DEFAULT NULL,
                          `FloatValue` float NOT NULL,
                          `FloatNullValue` float DEFAULT NULL,
                          `IntValue` int(11) NOT NULL,
                          `IntNullValue` int(11) DEFAULT NULL,
                          `IntegerValue` int(11) NOT NULL,
                          `IntegerNullValue` varchar(255) DEFAULT NULL,
                          `LinestringNullValue` linestring DEFAULT NULL,
                          `LongtextNullValue` varchar(255) DEFAULT NULL,
                          `MediumintValue` mediumint(9) NOT NULL,
                          `MediumintNullValue` mediumint(9) DEFAULT NULL,
                          `MediumtextNullValue` mediumtext,
                          `NcharNullValue` char(255) DEFAULT NULL,
                          `NumericValue` decimal(10,0) NOT NULL,
                          `NumericNullValue` decimal(10,0) DEFAULT NULL,
                          `NvarcharNullValue` varchar(255) DEFAULT NULL,
                          `RealValue` double NOT NULL,
                          `RealNullValue` double DEFAULT NULL,
                          `SmallintValue` smallint(6) NOT NULL,
                          `SmallintNullValue` smallint(6) DEFAULT NULL,
                          `TextNullValue` text,
                          `TimeNullValue` time DEFAULT NULL,
                          `TimestampValue` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                          `TimestampNullValue` timestamp NULL DEFAULT '0000-00-00 00:00:00',
                          `TinyintValue` tinyint(4) NOT NULL,
                          `TinyintNullValue` tinyint(4) DEFAULT NULL,
                          `TinytextNullValue` tinytext,
                          `VarbinaryNullValue` varbinary(255) DEFAULT NULL,
                          `VarcharNullValue` varchar(255) DEFAULT NULL,
                          `BinaryNullValue` binary(20) DEFAULT NULL
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8;"); 
            Default.RegisteCreateTableSQL(@"CREATE TABLE `time_test` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `user` varchar(5) COLLATE utf8_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;");
        }
        public MySQLHelper(string connectionString) : base(connectionString)
        {
        }
    }
}
