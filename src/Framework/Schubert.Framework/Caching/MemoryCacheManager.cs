using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Schubert.Framework.Caching
{
    /// <summary>
    /// 本机内存缓存（为了减少排他锁对性能造成的影响，该缓存实现为乐观并发模式）。
    /// </summary>
    public class MemoryCacheManager : ICacheManager, IDisposable
    {
        private ConcurrentDictionary<String, HashSet<String>> _caches = null;
        public const string DefaultCacheName = "sf.default";
        private MemoryCache _cache = null;

        internal IMemoryCache InnerCache { get { return _cache; } }
        

        public MemoryCacheManager(IOptions<MemoryCacheOptions> options)
            : this(options.Value)
        {
        }

        internal protected MemoryCacheManager(MemoryCacheOptions options)
        {
            _caches = new ConcurrentDictionary<string, HashSet<string>>();
            _cache = new MemoryCache(options);
        }

        private void ValidateRegion(string region, string argumentName)
        {
            if (region.IsNullOrWhiteSpace())
            {
                return;
            }
            if (region.Contains('|'))
            {
                throw new ArgumentException(@"缓存区域名称中不能包含字符串 ""|"" 。", argumentName);
            }
        }

        private string GetFullKey(string region, string key)
        {
            region = region.IfNullOrWhiteSpace(DefaultCacheName);
            return $"{region}|{key}";
        }

        private string GetRegionName(string region)
        {
            return region.IfNullOrWhiteSpace(DefaultCacheName);
        }

        private string GetRegionNameFormFullKey(string fullKey)
        {
            return fullKey.Split('|')[0];
        }

        private MemoryCacheEntryOptions CreateTimeoutOptions(TimeSpan? cacheTime, bool useSlidingExpiration)
        {
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
            if (!useSlidingExpiration)
            {
                options.AbsoluteExpirationRelativeToNow = cacheTime;
            }
            else
            {
                options.SlidingExpiration = cacheTime;
            }
            return options;
        }

        private void Callback(string key, object item, EvictionReason reason, object state)
        {
            string region = this.GetRegionNameFormFullKey(key);
            switch (reason)
            {
                case EvictionReason.Capacity:
                case EvictionReason.Removed:
                case EvictionReason.Expired:
                    HashSet<String> regionKeys = null;
                    if (_caches.TryGetValue(key, out regionKeys))
                    {
                        regionKeys.Remove(key);
                    }
                    break;
                default:
                    break;
            }
        }

        public void Clear()
        {
            KeyValuePair<String, HashSet<String>>[] caches = _caches.ToArray();
            foreach (var value in caches)
            {
                string[] keys = value.Value.ToArray();
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }
            }
        }
        
        public void ClearRegion(string region = "")
        {
            this.ValidateRegion(region, nameof(region));

            string name = GetRegionName(region);
            HashSet<string> regionKeys = null;
            if (_caches.TryGetValue(name, out regionKeys))
            {
                if (regionKeys != null)
                {
                    var keys = regionKeys.ToArray();
                    foreach (string key in keys)
                    {
                        _cache.Remove(key);
                    }
                }
            }
        }

        public object Get(string key, string region = "")
        {
            Guard.ArgumentNullOrWhiteSpaceString(key, nameof(key));
            this.ValidateRegion(region, nameof(region));

            string name = GetRegionName(region);
            string fullKey = this.GetFullKey(name, key);

            return _cache.Get(fullKey);
        }

        public void Remove(string key, string region = "")
        {
            this.ValidateRegion(region, nameof(region));

            string name = GetRegionName(region);
            string fullKey = this.GetFullKey(name, key);

            _cache.Remove(fullKey);
        }

        public void Set(string key, object data, TimeSpan? timeout = default(TimeSpan?), string region = "", bool useSlidingExpiration = false)
        {
            Guard.ArgumentNullOrWhiteSpaceString(key, nameof(key));
            this.ValidateRegion(region, nameof(region));
            if (data == null)
            {
                this.Remove(key, region);
                return;
            }
 
            string name = GetRegionName(region);
            string fullKey = this.GetFullKey(name, key);

            HashSet<String> regionKeys = _caches.GetOrAdd(name, n => new HashSet<string>());
            regionKeys.Add(fullKey);
            _cache.Set(fullKey, data, CreateTimeoutOptions(timeout, useSlidingExpiration)) ;
        }
        
        public void Dispose()
        {
            if (_cache != null)
            {
                _cache.Dispose();
            }
        }

        public bool Refresh(string key, string region = "")
        {
            var result = this.Get(key, region);
            return result != null;
        }
    }
}
