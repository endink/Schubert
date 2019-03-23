using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Schubert.Framework.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.Caching
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class MemoryCacheManagerTest : IDisposable
    {
        private MemoryCacheManager _manager = null;

        public MemoryCacheManagerTest()
        {
            var fac = new OptionsFactory<MemoryCacheOptions>(
                Enumerable.Empty<IConfigureOptions<MemoryCacheOptions>>(),
                Enumerable.Empty<IPostConfigureOptions<MemoryCacheOptions>>());

            IOptions<MemoryCacheOptions> options =
                new OptionsManager<MemoryCacheOptions>(fac);

            _manager = new MemoryCacheManager(options);
        }

        public void Dispose()
        {
            _manager.Dispose();
        }

        [Theory]
        [InlineData("A", "BB")]
        [InlineData("B", "CC")]
        [InlineData("A", "AA")]
        [InlineData("D", "BB")]
        public void SetTest(string key, string region)
        {
            _manager.Set(key, 3, region: region);

            Assert.Equal(3, _manager.Get(key, region));
        }

        [Theory]
        [InlineData("A", "BB")]
        [InlineData("B", "CC")]
        [InlineData("A", "AA")]
        [InlineData("D", "BB")]
        public void RemoveTest(string key, string region)
        {
            _manager.Set(key, new object(), region: region);

            Assert.NotNull(_manager.Get(key, region));

            _manager.Remove(key, region);
            Assert.Null(_manager.Get(key, region));

        }

        [Theory]
        [InlineData("A", "BB")]
        [InlineData("B", "CC")]
        [InlineData("A", "AA")]
        [InlineData("D", "BB")]
        public void ClearTest(string key, string region)
        {
            _manager.Set(key, new object(), region: region);

            Assert.NotNull(_manager.Get(key, region));

            _manager.Clear();
            Assert.Null(_manager.Get(key, region));

        }

        [Theory]
        [InlineData("A", "BB")]
        [InlineData("B", "CC")]
        [InlineData("A", "AA")]
        [InlineData("D", "BB")]
        public void ClearRegionTest(string key, string region)
        {
            _manager.Set(key, new object(), region: region);

            Assert.NotNull(_manager.Get(key, region));

            _manager.ClearRegion(region);
            Assert.Null(_manager.Get(key, region));

        }
    }
}
