using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Schubert.Framework.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.Caching
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class RedisCacheManagerTest : IDisposable
    {
        private RedisCacheManager _manager = null;
        private bool _userCompress = false;

        public RedisCacheManagerTest()
        {
            CreateManager();
        }

        private void CreateManager(bool compress = false)
        {
            ConfigureOptions<SchubertRedisOptions> prop = new ConfigureOptions<SchubertRedisOptions>(op =>
            {
                op.ConnectionString = "127.0.0.1:6379,abortConnect=false,ssl=false";
                //op.ConnectionString = "10.66.126.116:19000,abortConnect = false,ssl = false,password = Setpay123,connectTimeout = 10000,syncTimeout = 120000";
                op.GZipCompress = compress;
                op.SerializerName = RedisSerializerNames.JsonNet;
            });

            IOptions<SchubertRedisOptions> options =
                new OptionsManager<SchubertRedisOptions>(new OptionsFactory<SchubertRedisOptions>(new IConfigureOptions<SchubertRedisOptions>[]{ prop },
                new IPostConfigureOptions<SchubertRedisOptions>[0]));

            var sop = new ConfigureOptions<SchubertOptions>(op =>
            {
                op.AppSystemName = "testapp";
                op.Group = "jcgjz";
            });

            IOptions <SchubertOptions> schubertOptions =
                new OptionsManager<SchubertOptions>(new OptionsFactory<SchubertOptions>(new IConfigureOptions<SchubertOptions>[] { sop },
                new IPostConfigureOptions<SchubertOptions>[0]));

            _manager = new RedisCacheManager(schubertOptions, options);
        }

        private void ChangeMode(bool useCompress)
        {
            if (this._userCompress != useCompress)
            {
                _manager?.Dispose();
                CreateManager(true);
            }
        }

        public void Dispose()
        {
            _manager.Dispose();
        }

        [Theory(DisplayName = "RedisCacheManager: 复杂对象存取测试。")]
        [InlineData(true)]
        [InlineData(false)]
        public void Complex(bool useCompress)
        {
            _manager.Clear();
            this.ChangeMode(useCompress);
            var obj = _manager.Get("complext");
            Assert.Null(null);

            TestObject setObj = new TestObject();
            setObj.Enum = AttributeTargets.Event;
            setObj.Collection.Add(new TestObjectComplexPropery { A = "777" });

            _manager.Set("complex", setObj);
            var getObj = _manager.Get("complex") as TestObject;
            Assert.NotNull(getObj);
            Assert.Equal(1, getObj.Collection.Count);
        }

        [Theory(DisplayName = "RedisCacheManager: 缓存 Set 测试")]
        [InlineData("A", "BB")]
        [InlineData("B", "CC")]
        [InlineData("A", "AA")]
        [InlineData("D", "BB")]
        public void SetTest(string key, string region)
        {
            _manager.Set(key, new int[] { 3, 2, 1 }, region: region);

            var value = (int[])_manager.Get(key, region);
            Assert.Equal(3, value[0]);
            Assert.Equal(2, value[1]);
            Assert.Equal(1, value[2]);

            _manager.Remove(key);
        }

        [Theory(DisplayName = "RedisCacheManager: 缓存删除测试")]
        [InlineData("A", "BB")]
        [InlineData("B", "CC")]
        [InlineData("A", "AA")]
        [InlineData("D", "BB")]
        public void RemoveTest(string key, string region)
        {
            _manager.Clear();
            _manager.Set(key, "OOOO", region: region);

            Assert.NotNull(_manager.Get(key, region));

            _manager.Remove(key, region);
            Assert.Null(_manager.Get(key, region));

        }

        [Fact(DisplayName = "RedisCacheManager: 缓存刷新测试")]
        public void RefreshTest()
        {
            _manager.Clear();
            _manager.Set("A", 321, TimeSpan.FromSeconds(3), "B", true);
            object value;
            for (int i = 0; i <= 5; i++)
            {
                Thread.Sleep(1000);
                value = _manager.Refresh("A", "B");
            }
            value = _manager.Get("A", "B");
            Assert.Equal(321, value);
        }

        [Fact(DisplayName = "RedisCacheManager: 缓存滑动过期时间测试")]
        public void SlidingTimeTest()
        {
            _manager.Clear();
            _manager.Set("A", 321, TimeSpan.FromSeconds(3), "B", true);
            object value;
            for (int i = 0; i <= 5; i++)
            {
                Thread.Sleep(1000);
                value = _manager.Get("A", "B");
                Assert.Equal(321, value);
            }

            _manager.Set("B", 456, TimeSpan.FromSeconds(3), "B", true);
            Thread.Sleep(5000);
            value = _manager.Get("B", "B");
            Assert.NotEqual(456, value);
        }

        [Fact(DisplayName = "RedisCacheManager: 缓存清理测试")]
        public void ClearTest()
        {
            _manager.Clear();
            _manager.Set("aa", "RegionTest", region: "A");
            _manager.Set("bb", "RegionTest", region: "A");
            _manager.Set("cc", "RegionTest", region: "B");
            var value1 = _manager.Get("aa", "A");
            var value2 = _manager.Get("bb", "A");
            var value3 = _manager.Get("cc", "B");
            Assert.NotNull(value1);
            Assert.NotNull(value2);
            Assert.NotNull(value3);

            _manager.Clear();
            Assert.Null(_manager.Get("aa", "A"));
            Assert.Null(_manager.Get("bb", "A"));
            Assert.Null(_manager.Get("cc", "B"));

            _manager.Clear();
        }

        [Fact(DisplayName = "RedisCacheManager: 缓存区域清理测试")]
        public void ClearRegionTest()
        {
            _manager.Clear();
            _manager.Set("aa", "RegionTest", region: "A");
            _manager.Set("bb", "RegionTest", region: "A");
            var value1 = _manager.Get("aa", "A");
            var value2 = _manager.Get("bb", "A");
            Assert.NotNull(value1);
            Assert.NotNull(value2);

            _manager.ClearRegion("A");
            Assert.Null(_manager.Get("aa", "A"));
            Assert.Null(_manager.Get("bb", "A"));
            //无分区时删除不能出错。
            _manager.ClearRegion("A");

        }

        public class TestObject
        {
            public TestObject()
            {

                this.String = "StringValue";
                this.Int = int.MaxValue;
                this.NullInt = -100;
                this.Enum = AttributeTargets.Class;
                this.Complex = new TestObjectComplexPropery();
                this.Collection = new List<TestObjectComplexPropery>();
            }

            public String String { get; set; } = "666";

            public int Int { get; set; }

            public int? NullInt { get; set; }

            public AttributeTargets Enum { get; set; }

            public TestObjectComplexPropery Complex { get; set; }

            public ICollection<TestObjectComplexPropery> Collection { get; set; }
        }

        public class TestObjectComplexPropery
        {
            public String A { get; set; } = "666";

            public int B { get; set; }

            public int? BN { get; set; }

            public FlagsAttribute Attribute { get; set; }
        }
    }
}
