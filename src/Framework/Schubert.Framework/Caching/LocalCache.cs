using Microsoft.Extensions.Caching.Memory;
using System;

namespace Schubert.Framework.Caching
{
    /// <summary>
    /// 本地缓存对象（相对于分布式缓存）。
    /// </summary>
    public static class LocalCache
    {
        private static ICacheManager _localCache;
        private static readonly Object SyncRoot = new Object();

        /// <summary>
        /// 获取本地缓存对象实例。
        /// </summary>
        public static ICacheManager Current
        {
            get
            {
                if (_localCache == null)
                {
                    lock (SyncRoot)
                    {
                        if (_localCache == null)
                        {
                            _localCache = new MemoryCacheManager(new MemoryCacheOptions
                            { 
                                ExpirationScanFrequency = TimeSpan.FromMinutes(30)
                            });
                        }
                    }
                }
                return _localCache;
            }
        }

        public static IMemoryCache InnerCache
        {
            get { return (Current as MemoryCacheManager)?.InnerCache; }
        }
    }
}
