using System;

namespace Schubert.Framework.Caching
{
    public sealed class NullCacheManager : ICacheManager
    {

        public void Clear() { }

        public void ClearRegion(string region) { }

        public object Get(string key, string region = "")
        {
            return null;
        }

        public bool Refresh(string key, string region = "")
        {
            return false;
        }

        public void Remove(string key, string region = "")
        {
            
        }

        public void Set(string key, object data, TimeSpan? timeout = default(TimeSpan?), string region = "", bool useSlidingExpiration = false)
        {
            
        }
    }
}