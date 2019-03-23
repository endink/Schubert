using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Schubert
{
    public class ConvertibleString
    {
        private string _value = null;
        private static string _errorMessage = null;

        static ConvertibleString()
        {
            _errorMessage = (new InvalidCastException()).Message;
        }

        public ConvertibleString(string value)
        {
            this._value = value;
        }

        public override string ToString()
        {
            return this._value;
        }

        #region implicit

        #region primitive


        public static implicit operator String(ConvertibleString obj)
        {
            return obj._value;
        }

        public static implicit operator Int16(ConvertibleString obj)
        {
            Int16 result = default(Int16);
            Int16.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator UInt16(ConvertibleString obj)
        {
            UInt16 result = default(UInt16);
            UInt16.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator Int32(ConvertibleString obj)
        {
            Int32 result = default(Int32);
            Int32.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator UInt32(ConvertibleString obj)
        {
            UInt32 result = default(UInt32);
            UInt32.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator Int64(ConvertibleString obj)
        {
            Int64 result = default(Int64);
            Int64.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator UInt64(ConvertibleString obj)
        {
            UInt64 result = default(UInt64);
            UInt64.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator Byte(ConvertibleString obj)
        {
            Byte result = default(Byte);
            Byte.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator SByte(ConvertibleString obj)
        {
            SByte result = default(SByte);
            SByte.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator Decimal(ConvertibleString obj)
        {
            Decimal result = default(Decimal);
            Decimal.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator Double(ConvertibleString obj)
        {
            Double result = default(Double);
            Double.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator float(ConvertibleString obj)
        {
            float result = default(float);
            float.TryParse(obj._value, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static implicit operator Char(ConvertibleString obj)
        {
            Char result = default(Char);
            Char.TryParse(obj._value, out result);
            return result;
        }

        public static implicit operator Boolean(ConvertibleString obj)
        {
            Boolean result = default(Boolean);
            Boolean.TryParse(obj._value, out result);
            return result;
        }

        public static implicit operator DateTime(ConvertibleString obj)
        {
            DateTime result = default(DateTime);
            DateTime.TryParse(obj._value, System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out result);
            return result;
        }

        public static implicit operator Guid(ConvertibleString obj)
        {
            Guid result = default(Guid);
            Guid.TryParse(obj._value, out result);
            return result;
        }

        public static implicit operator byte[](ConvertibleString obj)
        {
            if (obj._value == null)
            {
                return null;
            }
            else
            {
                return Convert.FromBase64String(obj._value);
            }
        }

        #endregion

        #region nullable

        public static implicit operator Int16?(ConvertibleString obj)
        {
            Int16 result;
            if (Int16.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator UInt16?(ConvertibleString obj)
        {
            UInt16 result;
            if (UInt16.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator Int32?(ConvertibleString obj)
        {
            Int32 result;
            if (Int32.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator UInt32?(ConvertibleString obj)
        {
            UInt32 result;
            if (UInt32.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator Int64?(ConvertibleString obj)
        {
            Int64 result;
            if (Int64.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator UInt64?(ConvertibleString obj)
        {
            UInt64 result;
            if (UInt64.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator Byte?(ConvertibleString obj)
        {
            Byte result;
            if (Byte.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator SByte?(ConvertibleString obj)
        {
            SByte result;
            if (SByte.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator Decimal?(ConvertibleString obj)
        {
            Decimal result;
            if (Decimal.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator Double?(ConvertibleString obj)
        {
            Double result;
            if (Double.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator Single?(ConvertibleString obj)
        {
            float result;
            if (Single.TryParse(obj._value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator Char?(ConvertibleString obj)
        {
            Char result;
            if (Char.TryParse(obj._value, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator Boolean?(ConvertibleString obj)
        {
            Boolean result;
            if (Boolean.TryParse(obj._value, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator DateTime?(ConvertibleString obj)
        {
            DateTime result;
            if (DateTime.TryParse(obj._value, out result))
            {
                return result;
            }
            return null;
        }

        public static implicit operator Guid?(ConvertibleString obj)
        {
            Guid result;
            if (Guid.TryParse(obj._value, out result))
            {
                return result;
            }
            return null;
        }

        #endregion

        #endregion
    }
}
