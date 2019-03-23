using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Schubert.Framework.Http.Query;

namespace Schubert.Framework.Test.Http
{
    public class QueryStringTest
    {
        [Fact(DisplayName ="QueryString: 简单参数拼接")]
        public void TestQueryString()
        {
            QueryString qs = new QueryString(new TestParameter() { Id = 1, Name ="n" });
            Assert.Equal("id1=1&name1=n", qs.ToString());
        }

        [Fact(DisplayName = "EnumerableQueryString: 多值集合拼接")]
        public void TestEnumerableQueryString()
        {
            EnumerableQueryString qs = new EnumerableQueryString("lable", new[] { "l1", "l2", "l3" });
            Assert.Equal("lable=l1&lable=l2&lable=l3", qs.ToString());
        }
    }

    public class TestParameter
    {
        [QueryStringParameter("id1", true)]
        public int Id { get; set; }

        [QueryStringParameter("name1", true)]
        public String Name { get; set; }
    }
}
