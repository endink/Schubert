using Schubert.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.Net
{
    public class HostAndPortTest
    {
        [Fact(DisplayName ="HostAndPort: Parse 测试")]
        public void TestParse()
        {
            var hp = HostAndPort.Parse("127.0.0.1:3333");
            Assert.Equal("127.0.0.1", hp.Host);
            Assert.Equal(3333, hp.Port);

            hp = HostAndPort.Parse("baidu.com:3333");
            Assert.Equal("baidu.com", hp.Host);
            Assert.Equal(3333, hp.Port);

            Assert.Throws<ArgumentException>(() => HostAndPort.Parse("abcdef"));
            Assert.Throws<ArgumentException>(() => HostAndPort.Parse("127.0.0.1"));
            Assert.Throws<ArgumentException>(() => HostAndPort.Parse("127.0.0.1:80000"));
            Assert.Throws<ArgumentException>(() => HostAndPort.Parse("127.0.0.1:-1"));
        }


        [Fact(DisplayName = "HostAndPort: TryParse 测试")]
        public void TestTryParse()
        {
            HostAndPort p;
            Assert.Equal(true, HostAndPort.TryParse("127.0.0.1:3333", out p));
            Assert.Equal(true, HostAndPort.TryParse("abcd:3333", out p));
            Assert.Equal(false, HostAndPort.TryParse("127.0.0.1", out p));
            Assert.Equal(false, HostAndPort.TryParse("333", out p));
            Assert.Equal(false, HostAndPort.TryParse("127.0.0.1:-1", out p));
            Assert.Equal(false, HostAndPort.TryParse("127.0.0.1:80000", out p));
        }

        [Fact(DisplayName = "HostAndPort: ParseMultiple 测试")]
        public void TestParseMultiple()
        {
            var hp = HostAndPort.ParseMultiple("127.0.0.1:3333");
            Assert.Equal(1, hp.Length);

            hp = HostAndPort.ParseMultiple("127.0.0.1:3333, 127.0.0.1:4444");
            Assert.Equal(2, hp.Length);

            hp = HostAndPort.ParseMultiple("127.0.0.1:3333, 127.0.0.1:4444,abc:5555");
            Assert.Equal(3, hp.Length);

            Assert.Throws<ArgumentException>(() => HostAndPort.ParseMultiple("abcdef"));
            Assert.Throws<ArgumentException>(() => HostAndPort.ParseMultiple("127.0.0.1"));
            Assert.Throws<ArgumentException>(() => HostAndPort.ParseMultiple("127.0.0.1:80000"));
            Assert.Throws<ArgumentException>(() => HostAndPort.ParseMultiple("127.0.0.1:-1"));


            Assert.Throws<ArgumentException>(() => HostAndPort.ParseMultiple("127.0.0.1:3333, abcdef"));
            Assert.Throws<ArgumentException>(() => HostAndPort.ParseMultiple("127.0.0.1:3333, 127.0.0.1"));
            Assert.Throws<ArgumentException>(() => HostAndPort.ParseMultiple("127.0.0.1:3333, 127.0.0.1:80000"));
            Assert.Throws<ArgumentException>(() => HostAndPort.ParseMultiple("127.0.0.1:3333, 127.0.0.1:-1"));
        }
    }
}
