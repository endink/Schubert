using Schubert.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test
{
    public class TimeZoneHelperTest
    {
        [Theory(DisplayName = "TimeZoneHelper: 跨平台时区测试")]
        [InlineData("China Standard Time")]
        [InlineData("Asia/Shanghai")]
        public void TestGet(String name)
        {
            TimeZoneInfo zone = TimeZoneHelper.GetTimeZoneInfo(name);
        }
    }
}
