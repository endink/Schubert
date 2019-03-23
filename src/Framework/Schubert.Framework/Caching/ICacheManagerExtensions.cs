using System;
using System.Threading.Tasks;

namespace Schubert.Framework.Caching
{
    public static class ICacheManagerExtensions
    {
        public static T GetOrSet<T>(this ICacheManager manager, string key, Func<String, T> factory, TimeSpan? timeoutIfAdd = null, string region = "", bool useSlidingExpiration = false)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));

            object data = manager.Get(key, region);
            if (data == null)
            {
                data = factory(key);
                if (data != null)
                {
                    manager.Set(key, data, timeoutIfAdd, region, useSlidingExpiration);
                }
            }
            return (T)data;
        }

        public static async Task<T> GetOrSetAsync<T>(this ICacheManager manager, string key, Func<String, Task<T>> factory, TimeSpan? timeoutIfAdd = null, string region = "", bool useSlidingExpiration = false)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));

            object data = manager.Get(key, region);
            if (data == null)
            {
                data = await factory(key);
                if (data != null)
                {
                    manager.Set(key, data, timeoutIfAdd, region, useSlidingExpiration);
                }
            }
            return (T)data;
        }

        public static T Get<T>(this ICacheManager manager, string key, string region = "")
        {
            return (T)manager.Get(key, region);
        }
    }
}