using Newtonsoft.Json;
using Schubert.Framework.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.Json
{
    public class ExtendedCamelCaseContractResolverTester
    {
        public class TestObject : IEquatable<TestObject>
        {
            public DateTime? NullableDateTimeValue { get; set; }

            public DateTime DateTimeValue { get; set; }

            public DateTimeOffset? NullableDateTimeOffsetValue { get; set; }

            public DateTimeOffset DateTimeOffsetValue { get; set; }

            public long LongValue { get; set; }

            public long? NullableLongValue { get; set; }

            public override bool Equals(object obj)
            {
                return Equals(obj as TestObject);
            }

            public bool Equals(TestObject other)
            {
                return other != null &&
                       EqualityComparer<DateTime?>.Default.Equals(NullableDateTimeValue, other.NullableDateTimeValue) &&
                       DateTimeValue == other.DateTimeValue &&
                       EqualityComparer<DateTimeOffset?>.Default.Equals(NullableDateTimeOffsetValue, other.NullableDateTimeOffsetValue) &&
                       DateTimeOffsetValue.Equals(other.DateTimeOffsetValue) &&
                       LongValue == other.LongValue &&
                       EqualityComparer<long?>.Default.Equals(NullableLongValue, other.NullableLongValue);
            }

            public override int GetHashCode()
            {
                var hashCode = -894682935;
                hashCode = hashCode * -1521134295 + EqualityComparer<DateTime?>.Default.GetHashCode(NullableDateTimeValue);
                hashCode = hashCode * -1521134295 + DateTimeValue.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<DateTimeOffset?>.Default.GetHashCode(NullableDateTimeOffsetValue);
                hashCode = hashCode * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(DateTimeOffsetValue);
                hashCode = hashCode * -1521134295 + LongValue.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<long?>.Default.GetHashCode(NullableLongValue);
                return hashCode;
            }

            public static bool operator ==(TestObject object1, TestObject object2)
            {
                return EqualityComparer<TestObject>.Default.Equals(object1, object2);
            }

            public static bool operator !=(TestObject object1, TestObject object2)
            {
                return !(object1 == object2);
            }


        }

        [Fact(DisplayName = "ExtendedCamelCaseContractResolver: 处理 long 和 datetime")]
        public void Test()
        {
            TestObject t1 = new TestObject()
            {
                DateTimeOffsetValue = DateTimeOffset.Now,
                NullableDateTimeOffsetValue = DateTimeOffset.Now.AddDays(1),
                DateTimeValue = DateTime.Now.AddMinutes(3),
                NullableDateTimeValue = DateTime.Now.AddHours(1),
                LongValue = 9898989L,
                NullableLongValue = 7777777L
            };

            var resolver = new JsonResolverSettings();
            resolver.DateTimeToString.Enable = true;
            resolver.LongToString.Enable = true;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new ExtendedCamelCaseContractResolver(resolver);

            var json = JsonConvert.SerializeObject(t1, settings);
            var t11 = JsonConvert.DeserializeObject<TestObject>(json, settings);


            TestObject t2 = new TestObject()
            {
                DateTimeOffsetValue = DateTimeOffset.Now,
                NullableDateTimeOffsetValue = null,
                DateTimeValue = DateTime.Now.AddMinutes(3),
                NullableDateTimeValue = null,
                LongValue = 9898989L,
                NullableLongValue = null,
            };

            json = JsonConvert.SerializeObject(t2, settings);
            var t12 = JsonConvert.DeserializeObject<TestObject>(json, settings);
            
        }
    }
}
