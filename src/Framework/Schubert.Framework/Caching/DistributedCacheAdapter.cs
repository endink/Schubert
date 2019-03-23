using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Schubert.Framework.Caching
{
    public class DistributedCacheAdapter : IDistributedCache
    {
        private ICacheManager _cacheManager;
        public string _cacheRegion = null;
        public DistributedCacheAdapter(ICacheManager cacheManager, string cacheRegion)
        {
            Guard.ArgumentNotNull(cacheManager, nameof(cacheManager));
            _cacheManager = cacheManager;
            _cacheRegion = cacheRegion.IfNullOrWhiteSpace("aspnet");
        }

        public void Connect()
        {
            
        }

        public Task ConnectAsync()
        {
            return Task.FromResult(0);
        }

        public byte[] Get(string key)
        {
            return _cacheManager.Get(key, _cacheRegion) as byte[];
        }

        public void Refresh(string key)
        {
            _cacheManager.Refresh(key, _cacheRegion);
        }

        public void Remove(string key)
        {
            this._cacheManager.Remove(key, _cacheRegion);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (options.SlidingExpiration.HasValue)
            {
                _cacheManager.Set(key, value, options.SlidingExpiration, _cacheRegion, true);
            }
            else
            {
                _cacheManager.Set(key, value, options.AbsoluteExpirationRelativeToNow, _cacheRegion, false);
            }
        }
        

        public Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            return Task.Run(()=>this.Get(key), token);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            return Task.Run(() => this.Set(key, value, options), token);
        }

        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            return Task.Run(() => this.Remove(key), token);
        }

        public Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            return Task.Run(() => this.Remove(key), token);
        }
    }
}
