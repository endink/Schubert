using System;
using System.Data.Common;
using Xunit;

namespace Schubert.Framework.Test.Dapper
{
    static class CompareHelper
    {
        /// <summary>
        /// 测试
        /// </summary> 
        private static DbDataReader ColumnValueCompare(this DbDataReader reader, string column, object target)
        {
            var value = reader[column];
            if (value == null && target == null) return reader;
            if (target is DateTime)
            {
                if (DateTime.TryParse(value + "", out var date))
                {
                    Assert.Equal((DateTime)target, date);
                }
                else
                {
                    Assert.Equal((value + "").TrimEnd(), target + "");
                }
            }
            else if (target is float)
            {
                if (float.TryParse(value + "", out var f))
                {
                    Assert.Equal(f.ToString("0f"), ((float)target).ToString("0f"));
                }
                else
                {
                    Assert.Equal((value + "").TrimEnd(), target + "");
                }
            }
            else
            {
                Assert.Equal((value + "").TrimEnd(), target + "");
            }
            return reader;
        }
        private static DbDataReader ColumnValueCompare<T>(this DbDataReader reader, string column, Func<object, T> converter, T target)
        {
            Assert.Equal(converter(reader[column]), target);
            return reader;
        }

        public static void Compare(DbDataReader reader, DapperCompisiteKeyEntity entity)
        {
            reader.ColumnValueCompare("products_id", entity.products_id)
                .ColumnValueCompare("language_id", entity.language_id)
                .ColumnValueCompare("products_name", entity.products_name)
                .ColumnValueCompare("products_description", entity.products_description)
                .ColumnValueCompare("products_short_description", entity.products_short_description)
                .ColumnValueCompare("products_url", entity.products_url)
                .ColumnValueCompare("products_viewed", entity.products_viewed);
        }

        public static void Compare(DbDataReader reader, DapperTestEntity entity)
        {
            reader.ColumnValueCompare("id", Convert.ToInt32, entity.id)
                .ColumnValueCompare("bigint_value", entity.bigint_value)
                .ColumnValueCompare("bigint_null_value", entity.bigint_null_value)
                .ColumnValueCompare("bit_value", Convert.ToBoolean, entity.bit_value)
                .ColumnValueCompare("bit_null_value", entity.bit_null_value)
                .ColumnValueCompare("bool_value", entity.bool_value)
                .ColumnValueCompare("bool_null_value", entity.bool_null_value)
                .ColumnValueCompare("boolean_value", entity.boolean_value)
                .ColumnValueCompare("boolean_null_value", entity.boolean_null_value)
                .ColumnValueCompare("char_null_value", entity.char_null_value)
                .ColumnValueCompare("date_value", dt => dt == null ? null : Convert.ToDateTime(dt).ToString("yyyy-MM-dd"), entity.date_value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("date_null_value", entity.date_null_value == null ? string.Empty : entity.date_null_value.Value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("datetime_value", entity.datetime_value)
                .ColumnValueCompare("datetime_null_value", entity.datetime_null_value)
                .ColumnValueCompare("dec_value", entity.dec_value)
                .ColumnValueCompare("dec_null_value", entity.dec_null_value)
                .ColumnValueCompare("decimal_value", entity.decimal_value)
                .ColumnValueCompare("decimal_null_value", entity.decimal_null_value)
                .ColumnValueCompare("double_value", entity.double_value)
                .ColumnValueCompare("double_null_value", entity.double_null_value)
                .ColumnValueCompare("fix_value", entity.fix_value)
                .ColumnValueCompare("fix_null_value", entity.fix_null_value)
                .ColumnValueCompare("float_value", entity.float_value)
                .ColumnValueCompare("float_null_value", entity.float_null_value)
                .ColumnValueCompare("int_value", entity.int_value)
                .ColumnValueCompare("int_null_value", entity.int_null_value)
                .ColumnValueCompare("integer_value", entity.integer_value)
                .ColumnValueCompare("integer_null_value", entity.integer_null_value)
                .ColumnValueCompare("longtext_null_value", entity.longtext_null_value)
                .ColumnValueCompare("mediumint_value", entity.mediumint_value)
                .ColumnValueCompare("mediumint_null_value", entity.mediumint_null_value)
                .ColumnValueCompare("mediumtext_null_value", entity.mediumtext_null_value)
                .ColumnValueCompare("nchar_null_value", entity.nchar_null_value)
                .ColumnValueCompare("numeric_value", entity.numeric_value)
                .ColumnValueCompare("numeric_null_value", entity.numeric_null_value)
                .ColumnValueCompare("nvarchar_null_value", entity.nvarchar_null_value)
                .ColumnValueCompare("real_value", entity.real_value)
                .ColumnValueCompare("real_null_value", entity.real_null_value)
                .ColumnValueCompare("smallint_value", entity.smallint_value)
                .ColumnValueCompare("smallint_null_value", entity.smallint_null_value)
                .ColumnValueCompare("text_null_value", entity.text_null_value)
                .ColumnValueCompare("tinyint_value", entity.tinyint_value)
                .ColumnValueCompare("tinyint_null_value", entity.tinyint_null_value)
                .ColumnValueCompare("tinytext_null_value", entity.tinytext_null_value)
                .ColumnValueCompare("varchar_null_value", entity.varchar_null_value ?? string.Empty);
            if (entity.binary_null_value == null)
            {
                reader.ColumnValueCompare("binary_null_value", string.Empty);
            }
            if (entity.varbinary_null_value != null)
            {
                reader.ColumnValueCompare("varbinary_null_value",
                    bytes => System.Text.Encoding.Default.GetString((byte[])bytes),
                    System.Text.Encoding.Default.GetString(entity.varbinary_null_value));
            }
            else
            {
                reader.ColumnValueCompare("varbinary_null_value", string.Empty);
            }
        }
        public static void Compare(DbDataReader reader, SQLiteDapperTestEntity entity)
        {
            reader.ColumnValueCompare("id", Convert.ToInt32, entity.id)
                .ColumnValueCompare("bigint_value", entity.bigint_value)
                .ColumnValueCompare("bigint_null_value", entity.bigint_null_value)
                .ColumnValueCompare("char_null_value", entity.char_null_value)
                .ColumnValueCompare("date_value", dt => dt == null ? null : Convert.ToDateTime(dt).ToString("yyyy-MM-dd"), entity.date_value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("date_null_value", entity.date_null_value == null ? string.Empty : entity.date_null_value.Value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("datetime_value", entity.datetime_value)
                .ColumnValueCompare("datetime_null_value", entity.datetime_null_value)
                .ColumnValueCompare("dec_value", entity.dec_value)
                .ColumnValueCompare("dec_null_value", entity.dec_null_value)
                .ColumnValueCompare("decimal_value", entity.decimal_value)
                .ColumnValueCompare("decimal_null_value", entity.decimal_null_value)
                .ColumnValueCompare("double_value", entity.double_value)
                .ColumnValueCompare("double_null_value", entity.double_null_value)
                .ColumnValueCompare("fix_value", entity.fix_value)
                .ColumnValueCompare("fix_null_value", entity.fix_null_value)
                .ColumnValueCompare("float_value", entity.float_value)
                .ColumnValueCompare("float_null_value", entity.float_null_value)
                .ColumnValueCompare("int_value", entity.int_value)
                .ColumnValueCompare("int_null_value", entity.int_null_value)
                .ColumnValueCompare("integer_value", entity.integer_value)
                .ColumnValueCompare("integer_null_value", entity.integer_null_value)
                .ColumnValueCompare("longtext_null_value", entity.longtext_null_value)
                .ColumnValueCompare("mediumint_value", entity.mediumint_value)
                .ColumnValueCompare("mediumint_null_value", entity.mediumint_null_value)
                .ColumnValueCompare("mediumtext_null_value", entity.mediumtext_null_value)
                .ColumnValueCompare("nchar_null_value", entity.nchar_null_value)
                .ColumnValueCompare("numeric_value", entity.numeric_value)
                .ColumnValueCompare("numeric_null_value", entity.numeric_null_value)
                .ColumnValueCompare("nvarchar_null_value", entity.nvarchar_null_value)
                .ColumnValueCompare("real_value", entity.real_value)
                .ColumnValueCompare("real_null_value", entity.real_null_value)
                .ColumnValueCompare("smallint_value", entity.smallint_value)
                .ColumnValueCompare("smallint_null_value", entity.smallint_null_value)
                .ColumnValueCompare("text_null_value", entity.text_null_value)
                .ColumnValueCompare("tinyint_value", entity.tinyint_value)
                .ColumnValueCompare("tinyint_null_value", entity.tinyint_null_value)
                .ColumnValueCompare("tinytext_null_value", entity.tinytext_null_value)
                .ColumnValueCompare("varchar_null_value", entity.varchar_null_value ?? string.Empty);
            if (entity.binary_null_value == null)
            {
                reader.ColumnValueCompare("binary_null_value", string.Empty);
            }
            if (entity.varbinary_null_value != null)
            {
                reader.ColumnValueCompare("varbinary_null_value",
                    bytes => System.Text.Encoding.Default.GetString((byte[])bytes),
                    System.Text.Encoding.Default.GetString(entity.varbinary_null_value));
            }
            else
            {
                reader.ColumnValueCompare("varbinary_null_value", string.Empty);
            }
        }
        public static void Compare(DbDataReader reader, DapperTest entity)
        {
            reader.ColumnValueCompare("id", Convert.ToInt32, entity.id)
                .ColumnValueCompare("bigint_value", entity.bigint_value)
                .ColumnValueCompare("bigint_null_value", entity.bigint_null_value)
                .ColumnValueCompare("char_null_value", entity.char_null_value)
                .ColumnValueCompare("date_value", dt => dt == null ? null : Convert.ToDateTime(dt).ToString("yyyy-MM-dd"), entity.date_value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("date_null_value", entity.date_null_value == null ? string.Empty : entity.date_null_value.Value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("datetime_value", entity.datetime_value)
                .ColumnValueCompare("datetime_null_value", entity.datetime_null_value)
                .ColumnValueCompare("dec_value", entity.dec_value)
                .ColumnValueCompare("dec_null_value", entity.dec_null_value)
                .ColumnValueCompare("decimal_value", entity.decimal_value)
                .ColumnValueCompare("decimal_null_value", entity.decimal_null_value)
                .ColumnValueCompare("double_value", entity.double_value)
                .ColumnValueCompare("double_null_value", entity.double_null_value)
                .ColumnValueCompare("fix_value", entity.fix_value)
                .ColumnValueCompare("fix_null_value", entity.fix_null_value)
                .ColumnValueCompare("float_value", entity.float_value)
                .ColumnValueCompare("float_null_value", entity.float_null_value)
                .ColumnValueCompare("int_value", entity.int_value)
                .ColumnValueCompare("int_null_value", entity.int_null_value)
                .ColumnValueCompare("integer_value", entity.integer_value)
                .ColumnValueCompare("integer_null_value", entity.integer_null_value)
                .ColumnValueCompare("longtext_null_value", entity.longtext_null_value)
                .ColumnValueCompare("mediumint_value", entity.mediumint_value)
                .ColumnValueCompare("mediumint_null_value", entity.mediumint_null_value)
                .ColumnValueCompare("mediumtext_null_value", entity.mediumtext_null_value)
                .ColumnValueCompare("nchar_null_value", entity.nchar_null_value)
                .ColumnValueCompare("numeric_value", entity.numeric_value)
                .ColumnValueCompare("numeric_null_value", entity.numeric_null_value)
                .ColumnValueCompare("nvarchar_null_value", entity.nvarchar_null_value)
                .ColumnValueCompare("real_value", entity.real_value)
                .ColumnValueCompare("real_null_value", entity.real_null_value)
                .ColumnValueCompare("smallint_value", entity.smallint_value)
                .ColumnValueCompare("smallint_null_value", entity.smallint_null_value)
                .ColumnValueCompare("text_null_value", entity.text_null_value)
                .ColumnValueCompare("tinyint_value", entity.tinyint_value)
                .ColumnValueCompare("tinyint_null_value", entity.tinyint_null_value)
                .ColumnValueCompare("tinytext_null_value", entity.tinytext_null_value)
                .ColumnValueCompare("varchar_null_value", entity.varchar_null_value ?? string.Empty);
            if (entity.binary_null_value == null)
            {
                reader.ColumnValueCompare("binary_null_value", string.Empty);
            }
            if (entity.varbinary_null_value != null)
            {
                reader.ColumnValueCompare("varbinary_null_value",
                    bytes => System.Text.Encoding.Default.GetString((byte[])bytes),
                    System.Text.Encoding.Default.GetString(entity.varbinary_null_value));
            }
            else
            {
                reader.ColumnValueCompare("varbinary_null_value", string.Empty);
            }
        }
        public static void Compare(DbDataReader reader, DapperEntityWithNoBool entity)
        {
            reader.ColumnValueCompare("id", Convert.ToInt32, entity.id)
                .ColumnValueCompare("bigint_value", entity.bigint_value)
                .ColumnValueCompare("bigint_null_value", entity.bigint_null_value)
                .ColumnValueCompare("char_null_value", entity.char_null_value)
                .ColumnValueCompare("date_value", dt => dt == null ? null : Convert.ToDateTime(dt).ToString("yyyy-MM-dd"), entity.date_value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("date_null_value", entity.date_null_value == null ? string.Empty : entity.date_null_value.Value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("datetime_value", entity.datetime_value)
                .ColumnValueCompare("datetime_null_value", entity.datetime_null_value)
                .ColumnValueCompare("dec_value", entity.dec_value)
                .ColumnValueCompare("dec_null_value", entity.dec_null_value)
                .ColumnValueCompare("decimal_value", entity.decimal_value)
                .ColumnValueCompare("decimal_null_value", entity.decimal_null_value)
                .ColumnValueCompare("double_value", entity.double_value)
                .ColumnValueCompare("double_null_value", entity.double_null_value)
                .ColumnValueCompare("fix_value", entity.fix_value)
                .ColumnValueCompare("fix_null_value", entity.fix_null_value)
                .ColumnValueCompare("float_value", entity.float_value)
                .ColumnValueCompare("float_null_value", entity.float_null_value)
                .ColumnValueCompare("int_value", entity.int_value)
                .ColumnValueCompare("int_null_value", entity.int_null_value)
                .ColumnValueCompare("integer_value", entity.integer_value)
                .ColumnValueCompare("integer_null_value", entity.integer_null_value)
                .ColumnValueCompare("longtext_null_value", entity.longtext_null_value)
                .ColumnValueCompare("mediumint_value", entity.mediumint_value)
                .ColumnValueCompare("mediumint_null_value", entity.mediumint_null_value)
                .ColumnValueCompare("mediumtext_null_value", entity.mediumtext_null_value)
                .ColumnValueCompare("nchar_null_value", entity.nchar_null_value)
                .ColumnValueCompare("numeric_value", entity.numeric_value)
                .ColumnValueCompare("numeric_null_value", entity.numeric_null_value)
                .ColumnValueCompare("nvarchar_null_value", entity.nvarchar_null_value)
                .ColumnValueCompare("real_value", entity.real_value)
                .ColumnValueCompare("real_null_value", entity.real_null_value)
                .ColumnValueCompare("smallint_value", entity.smallint_value)
                .ColumnValueCompare("smallint_null_value", entity.smallint_null_value)
                .ColumnValueCompare("text_null_value", entity.text_null_value)
                .ColumnValueCompare("tinyint_value", entity.tinyint_value)
                .ColumnValueCompare("tinyint_null_value", entity.tinyint_null_value)
                .ColumnValueCompare("tinytext_null_value", entity.tinytext_null_value)
                .ColumnValueCompare("varchar_null_value", entity.varchar_null_value ?? string.Empty);

        }
        public static void Compare(DbDataReader reader, DapperAllTest entity)
        {
            reader.ColumnValueCompare("id", Convert.ToInt32, entity.Id)
                .ColumnValueCompare("bigint_value", entity.BigintValue)
                .ColumnValueCompare("bigint_null_value", entity.BigintNullValue)
                .ColumnValueCompare("bit_value", Convert.ToBoolean, entity.BitValue)
                .ColumnValueCompare("bit_null_value", entity.BitNullValue)
                .ColumnValueCompare("bool_value", entity.BoolValue)
                .ColumnValueCompare("bool_null_value", entity.BoolNullValue)
                .ColumnValueCompare("boolean_value", entity.BooleanValue)
                .ColumnValueCompare("boolean_null_value", entity.BooleanNullValue)
                .ColumnValueCompare("char_null_value", entity.CharNullValue)
                .ColumnValueCompare("date_value", dt => dt == null ? null : Convert.ToDateTime(dt).ToString("yyyy-MM-dd"), entity.DateValue.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("date_null_value", entity.DateNullValue == null ? string.Empty : entity.DateNullValue.Value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("datetime_value", entity.DatetimeValue)
                .ColumnValueCompare("datetime_null_value", entity.DatetimeNullValue)
                .ColumnValueCompare("dec_value", entity.DecValue)
                .ColumnValueCompare("dec_null_value", entity.DecNullValue)
                .ColumnValueCompare("decimal_value", entity.DecimalValue)
                .ColumnValueCompare("decimal_null_value", entity.DecimalNullValue)
                .ColumnValueCompare("double_value", entity.DoubleValue)
                .ColumnValueCompare("double_null_value", entity.DoubleNullValue)
                .ColumnValueCompare("fix_value", entity.FixValue)
                .ColumnValueCompare("fix_null_value", entity.FixNullValue)
                .ColumnValueCompare("float_value", entity.FloatValue)
                .ColumnValueCompare("float_null_value", entity.FloatNullValue)
                .ColumnValueCompare("int_value", entity.IntValue)
                .ColumnValueCompare("int_null_value", entity.IntNullValue)
                .ColumnValueCompare("integer_value", entity.IntegerValue)
                .ColumnValueCompare("integer_null_value", entity.IntegerNullValue)
                .ColumnValueCompare("longtext_null_value", entity.LongtextNullValue)
                .ColumnValueCompare("mediumint_value", entity.MediumintValue)
                .ColumnValueCompare("mediumint_null_value", entity.MediumintNullValue)
                .ColumnValueCompare("mediumtext_null_value", entity.MediumtextNullValue)
                .ColumnValueCompare("nchar_null_value", entity.NcharNullValue)
                .ColumnValueCompare("numeric_value", entity.NumericValue)
                .ColumnValueCompare("numeric_null_value", entity.NumericNullValue)
                .ColumnValueCompare("nvarchar_null_value", entity.NvarcharNullValue)
                .ColumnValueCompare("real_value", entity.RealValue)
                .ColumnValueCompare("real_null_value", entity.RealNullValue)
                .ColumnValueCompare("smallint_value", entity.SmallintValue)
                .ColumnValueCompare("smallint_null_value", entity.SmallintNullValue)
                .ColumnValueCompare("text_null_value", entity.TextNullValue)
                .ColumnValueCompare("tinyint_value", entity.TinyintValue)
                .ColumnValueCompare("tinyint_null_value", entity.TinyintNullValue)
                .ColumnValueCompare("tinytext_null_value", entity.TinytextNullValue)
                .ColumnValueCompare("varchar_null_value", entity.VarcharNullValue ?? string.Empty);
            if (entity.BinaryNullValue == null)
            {
                reader.ColumnValueCompare("binary_null_value", string.Empty);
            }
            if (entity.VarbinaryNullValue != null)
            {
                reader.ColumnValueCompare("varbinary_null_value",
                    bytes => System.Text.Encoding.Default.GetString((byte[])bytes),
                    System.Text.Encoding.Default.GetString(entity.VarbinaryNullValue));
            }
            else
            {
                reader.ColumnValueCompare("varbinary_null_value", string.Empty);
            }
        }
        public static void ComparePascalCase(DbDataReader reader, DapperAllTest entity)
        {
            reader.ColumnValueCompare("Id", Convert.ToInt32, entity.Id)
                .ColumnValueCompare("BigintValue", entity.BigintValue)
                .ColumnValueCompare("BigintNullValue", entity.BigintNullValue)
                .ColumnValueCompare("BitValue", Convert.ToBoolean, entity.BitValue)
                .ColumnValueCompare("BitNullValue", entity.BitNullValue)
                .ColumnValueCompare("BoolValue", entity.BoolValue)
                .ColumnValueCompare("BoolNullValue", entity.BoolNullValue)
                .ColumnValueCompare("BooleanValue", entity.BooleanValue)
                .ColumnValueCompare("BooleanNullValue", entity.BooleanNullValue)
                .ColumnValueCompare("CharNullValue", entity.CharNullValue)
                .ColumnValueCompare("DateValue", dt => dt == null ? null : Convert.ToDateTime(dt).ToString("yyyy-MM-dd"), entity.DateValue.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("DateNullValue", entity.DateNullValue == null ? string.Empty : entity.DateNullValue.Value.ToString("yyyy-MM-dd"))
                .ColumnValueCompare("DatetimeValue", entity.DatetimeValue)
                .ColumnValueCompare("DatetimeNullValue", entity.DatetimeNullValue)
                .ColumnValueCompare("DecValue", entity.DecValue)
                .ColumnValueCompare("DecNullValue", entity.DecNullValue)
                .ColumnValueCompare("DecimalValue", entity.DecimalValue)
                .ColumnValueCompare("DecimalNullValue", entity.DecimalNullValue)
                .ColumnValueCompare("DoubleValue", entity.DoubleValue)
                .ColumnValueCompare("DoubleNullValue", entity.DoubleNullValue)
                .ColumnValueCompare("FixValue", entity.FixValue)
                .ColumnValueCompare("FixNullValue", entity.FixNullValue)
                .ColumnValueCompare("FloatValue", entity.FloatValue)
                .ColumnValueCompare("FloatNullValue", entity.FloatNullValue)
                .ColumnValueCompare("IntValue", entity.IntValue)
                .ColumnValueCompare("IntNullValue", entity.IntNullValue)
                .ColumnValueCompare("IntegerValue", entity.IntegerValue)
                .ColumnValueCompare("IntegerNullValue", entity.IntegerNullValue)
                .ColumnValueCompare("LongtextNullValue", entity.LongtextNullValue)
                .ColumnValueCompare("MediumintValue", entity.MediumintValue)
                .ColumnValueCompare("MediumintNullValue", entity.MediumintNullValue)
                .ColumnValueCompare("MediumtextNullValue", entity.MediumtextNullValue)
                .ColumnValueCompare("NcharNullValue", entity.NcharNullValue)
                .ColumnValueCompare("NumericValue", entity.NumericValue)
                .ColumnValueCompare("NumericNullValue", entity.NumericNullValue)
                .ColumnValueCompare("NvarcharNullValue", entity.NvarcharNullValue)
                .ColumnValueCompare("RealValue", entity.RealValue)
                .ColumnValueCompare("RealNullValue", entity.RealNullValue)
                .ColumnValueCompare("SmallintValue", entity.SmallintValue)
                .ColumnValueCompare("SmallintNullValue", entity.SmallintNullValue)
                .ColumnValueCompare("TextNullValue", entity.TextNullValue)
                .ColumnValueCompare("TinyintValue", entity.TinyintValue)
                .ColumnValueCompare("TinyintNullValue", entity.TinyintNullValue)
                .ColumnValueCompare("TinytextNullValue", entity.TinytextNullValue)
                .ColumnValueCompare("VarcharNullValue", entity.VarcharNullValue ?? string.Empty);
            if (entity.BinaryNullValue == null)
            {
                reader.ColumnValueCompare("BinaryNullValue", string.Empty);
            }
            if (entity.VarbinaryNullValue != null)
            {
                reader.ColumnValueCompare("VarbinaryNullValue",
                    bytes => System.Text.Encoding.Default.GetString((byte[])bytes),
                    System.Text.Encoding.Default.GetString(entity.VarbinaryNullValue));
            }
            else
            {
                reader.ColumnValueCompare("VarbinaryNullValue", string.Empty);
            }
        }
        public static void Compare(DapperCompisiteKeyEntity d1, DapperCompisiteKeyEntity entity)
        {
            Assert.Equal(d1.language_id.ToString(), entity.language_id.ToString());
            Assert.Equal(d1.products_description.ToString(), entity.products_description.ToString());
            Assert.Equal(d1.products_id.ToString(), entity.products_id.ToString());
            Assert.Equal(d1.products_name, entity.products_name);
            Assert.Equal(d1.products_short_description, entity.products_short_description);
            Assert.Equal(d1.products_url, entity.products_url);
            Assert.Equal(d1.products_viewed.ToString(), entity.products_viewed.ToString());
        }
    }
}
