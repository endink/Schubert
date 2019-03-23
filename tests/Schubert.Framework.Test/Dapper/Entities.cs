using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Test.Dapper
{
    public class DapperCompisiteKeyEntity
    {
        public int products_id { get; set; }
        public int language_id { get; set; }
        public string products_name { get; set; }
        public string products_description { get; set; }
        public string products_short_description { get; set; }
        public string products_url { get; set; }
        public int products_viewed { get; set; }
    }
    public enum Classic : int { }
    [Flags]
    public enum Classic2 : byte { }
    public class DapperTestEntity
    {
        public int id { get; set; }

        public long bigint_value { get; set; }

        public bool bit_value { get; set; }

        public bool bool_value { get; set; }

        public Boolean boolean_value { get; set; }

        public string char_null_value { get; set; }

        public DateTime date_value { get; set; }

        public DateTime datetime_value { get; set; }

        public decimal dec_value { get; set; }

        public decimal decimal_value { get; set; }

        public double double_value { get; set; }

        public decimal fix_value { get; set; }

        public float float_value { get; set; }

        public int int_value { get; set; }

        public int integer_value { get; set; }

        public string longtext_null_value { get; set; }

        public int mediumint_value { get; set; }


        public string mediumtext_null_value { get; set; }

        public string nchar_null_value { get; set; }

        public decimal numeric_value { get; set; }


        public string nvarchar_null_value { get; set; }

        public double real_value { get; set; }


        public int smallint_value { get; set; }


        public string text_null_value { get; set; }

        //public DateTime time_value { get; set; }


        //public TimeSpan timestamp_value { get; set; }


        public int tinyint_value { get; set; }

        public string tinytext_null_value { get; set; }

        public byte[] varbinary_null_value { get; set; }

        public string varchar_null_value { get; set; }

        public byte[] binary_null_value { get; set; }

        public long? bigint_null_value { get; set; }

        public Boolean? boolean_null_value { get; set; }

        public bool? bit_null_value { get; set; }

        public bool? bool_null_value { get; set; }

        public int? tinyint_null_value { get; set; }

        //public TimeSpan? timestamp_null_value { get; set; }

        //public TimeSpan? time_null_value { get; set; }

        public int? smallint_null_value { get; set; }

        public double? real_null_value { get; set; }

        public decimal? numeric_null_value { get; set; }

        public int? mediumint_null_value { get; set; }

        public int? integer_null_value { get; set; }

        public int? int_null_value { get; set; }

        public float? float_null_value { get; set; }

        public decimal? fix_null_value { get; set; }

        public double? double_null_value { get; set; }

        public decimal? decimal_null_value { get; set; }

        public decimal? dec_null_value { get; set; }

        public DateTime? datetime_null_value { get; set; }

        public DateTime? date_null_value { get; set; }
    }
    public class SQLiteDapperTestEntity
    {
        public int id { get; set; }

        public long bigint_value { get; set; }

        public string char_null_value { get; set; }

        public DateTime date_value { get; set; }

        public DateTime datetime_value { get; set; }

        public decimal dec_value { get; set; }

        public decimal decimal_value { get; set; }

        public double double_value { get; set; }

        public decimal fix_value { get; set; }

        public float float_value { get; set; }

        public int int_value { get; set; }

        public int integer_value { get; set; }

        public string longtext_null_value { get; set; }

        public int mediumint_value { get; set; }


        public string mediumtext_null_value { get; set; }

        public string nchar_null_value { get; set; }

        public decimal numeric_value { get; set; }


        public string nvarchar_null_value { get; set; }

        public double real_value { get; set; }


        public int smallint_value { get; set; }


        public string text_null_value { get; set; }


        public int tinyint_value { get; set; }

        public string tinytext_null_value { get; set; }

        public byte[] varbinary_null_value { get; set; }

        public string varchar_null_value { get; set; }

        public byte[] binary_null_value { get; set; }

        public long? bigint_null_value { get; set; }

        public int? tinyint_null_value { get; set; }

        public int? smallint_null_value { get; set; }

        public double? real_null_value { get; set; }

        public decimal? numeric_null_value { get; set; }

        public int? mediumint_null_value { get; set; }

        public int? integer_null_value { get; set; }

        public int? int_null_value { get; set; }

        public float? float_null_value { get; set; }

        public decimal? fix_null_value { get; set; }

        public double? double_null_value { get; set; }

        public decimal? decimal_null_value { get; set; }

        public decimal? dec_null_value { get; set; }

        public DateTime? datetime_null_value { get; set; }

        public DateTime? date_null_value { get; set; }
    }

    public class DapperTest
    {
        public int id { get; set; }

        public long bigint_value { get; set; }

        public string char_null_value { get; set; }

        public DateTime date_value { get; set; }

        public DateTime datetime_value { get; set; }

        public decimal dec_value { get; set; }

        public decimal decimal_value { get; set; }

        public double double_value { get; set; }

        public decimal fix_value { get; set; }

        public float float_value { get; set; }

        public int int_value { get; set; }

        public int integer_value { get; set; }

        public string longtext_null_value { get; set; }

        public int mediumint_value { get; set; }


        public string mediumtext_null_value { get; set; }

        public string nchar_null_value { get; set; }

        public decimal numeric_value { get; set; }


        public string nvarchar_null_value { get; set; }

        public double real_value { get; set; }


        public int smallint_value { get; set; }


        public string text_null_value { get; set; }


        public int tinyint_value { get; set; }

        public string tinytext_null_value { get; set; }

        public byte[] varbinary_null_value { get; set; }

        public string varchar_null_value { get; set; }

        public byte[] binary_null_value { get; set; }

        public long? bigint_null_value { get; set; }

        public int? tinyint_null_value { get; set; }

        public int? smallint_null_value { get; set; }

        public double? real_null_value { get; set; }

        public decimal? numeric_null_value { get; set; }

        public int? mediumint_null_value { get; set; }

        public int? integer_null_value { get; set; }

        public int? int_null_value { get; set; }

        public float? float_null_value { get; set; }

        public decimal? fix_null_value { get; set; }

        public double? double_null_value { get; set; }

        public decimal? decimal_null_value { get; set; }

        public decimal? dec_null_value { get; set; }

        public DateTime? datetime_null_value { get; set; }

        public DateTime? date_null_value { get; set; }
    }
    public class DapperAllTest
    {
        public int Id { get; set; }

        public long BigintValue { get; set; }

        public bool BitValue { get; set; }

        public bool BoolValue { get; set; }

        public Boolean BooleanValue { get; set; }

        public string CharNullValue { get; set; }

        public DateTime DateValue { get; set; }

        public DateTime DatetimeValue { get; set; }

        public decimal DecValue { get; set; }

        public decimal DecimalValue { get; set; }

        public double DoubleValue { get; set; }

        public decimal FixValue { get; set; }

        public float FloatValue { get; set; }

        public int IntValue { get; set; }

        public int IntegerValue { get; set; }

        public string LongtextNullValue { get; set; }

        public int MediumintValue { get; set; }


        public string MediumtextNullValue { get; set; }

        public string NcharNullValue { get; set; }

        public decimal NumericValue { get; set; }


        public string NvarcharNullValue { get; set; }

        public double RealValue { get; set; }


        public int SmallintValue { get; set; }


        public string TextNullValue { get; set; }

        //public DateTime TimeValue { get; set; }


        //public TimeSpan TimestampValue { get; set; }


        public int TinyintValue { get; set; }

        public string TinytextNullValue { get; set; }

        public byte[] VarbinaryNullValue { get; set; }

        public string VarcharNullValue { get; set; }

        public byte[] BinaryNullValue { get; set; }

        public long? BigintNullValue { get; set; }

        public Boolean? BooleanNullValue { get; set; }

        public bool? BitNullValue { get; set; }

        public bool? BoolNullValue { get; set; }

        public int? TinyintNullValue { get; set; }

        //public TimeSpan? TimestampNullValue { get; set; }

        //public TimeSpan? TimeNullValue { get; set; }

        public int? SmallintNullValue { get; set; }

        public double? RealNullValue { get; set; }

        public decimal? NumericNullValue { get; set; }

        public int? MediumintNullValue { get; set; }

        public int? IntegerNullValue { get; set; }

        public int? IntNullValue { get; set; }

        public float? FloatNullValue { get; set; }

        public decimal? FixNullValue { get; set; }

        public double? DoubleNullValue { get; set; }

        public decimal? DecimalNullValue { get; set; }

        public decimal? DecNullValue { get; set; }

        public DateTime? DatetimeNullValue { get; set; }

        public DateTime? DateNullValue { get; set; }
    }
    public class DapperEntityWithNoBool
    {
        public int id { get; set; }

        public long bigint_value { get; set; }

        public string char_null_value { get; set; }

        public DateTime date_value { get; set; }

        public DateTime datetime_value { get; set; }
        public DateTime? datetime_null_value { get; set; }

        public DateTime? date_null_value { get; set; }

        public decimal dec_value { get; set; }

        public decimal decimal_value { get; set; }

        public double double_value { get; set; }

        public decimal fix_value { get; set; }

        public float float_value { get; set; }

        public int int_value { get; set; }

        public int integer_value { get; set; }

        public string longtext_null_value { get; set; }

        public int mediumint_value { get; set; }


        public string mediumtext_null_value { get; set; }

        public string nchar_null_value { get; set; }

        public decimal numeric_value { get; set; }


        public string nvarchar_null_value { get; set; }

        public double real_value { get; set; }


        public int smallint_value { get; set; }


        public string text_null_value { get; set; }


        public int tinyint_value { get; set; }

        public string tinytext_null_value { get; set; }


        public string varchar_null_value { get; set; }

        public long? bigint_null_value { get; set; }


        public int? tinyint_null_value { get; set; }

        public int? smallint_null_value { get; set; }

        public double? real_null_value { get; set; }

        public decimal? numeric_null_value { get; set; }

        public int? mediumint_null_value { get; set; }

        public int? integer_null_value { get; set; }

        public int? int_null_value { get; set; }

        public float? float_null_value { get; set; }

        public decimal? fix_null_value { get; set; }

        public double? double_null_value { get; set; }

        public decimal? decimal_null_value { get; set; }

        public decimal? dec_null_value { get; set; }


    }
}
