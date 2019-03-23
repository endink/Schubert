using Schubert.Framework.Caching;
using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    public static class CacheExtensions
    {
        /// <summary>
        /// 从缓存中获取指定用户 Id 的用户数据，如果没有缓存，返回 null。
        /// </summary>
        /// <param name="cacheManager"></param>
        /// <param name="userId">用户 Id。</param>
        /// <returns></returns>
        public static object GetUser(this ICacheManager cacheManager, long userId)
        {
            string key = CacheKeyContracts.GetIdentityCacheKey(userId);
            return cacheManager.Get(key, String.Empty);
        }

        /// <summary>
        /// 将用户数据添加的缓存中。
        /// </summary>
        /// <typeparam name="TUser">用户对象的类型参数。</typeparam>
        /// <param name="cacheManager">缓存对象。</param>
        /// <param name="user">用户实例。</param>
        /// <param name="timeoutIfAdd">如果用户添加操作发生，该添加操作的缓存对象的超时时间。</param>
        /// <param name="useSlidingExpiration">是否使用滑动过期（即最后一次使用后 timeoutIfAdd 定义的时间内不再使用视为过期）。</param>
        public static void SetUser<TUser>(this ICacheManager cacheManager, TUser user, TimeSpan? timeoutIfAdd = null, bool useSlidingExpiration = false)
             where TUser : UserBase
        {
            if (user == null)
            {
                return;
            }
            string key = CacheKeyContracts.GetIdentityCacheKey(user.Id);
            cacheManager.Set(key, user, timeoutIfAdd, string.Empty, useSlidingExpiration);
        }

        /// <summary>
        /// 尝试从缓存中获取用户数据。如果缓存不存在用户数据，使用 <paramref name="userFactory"/> 提供的回调方法创建一个用户并添加到缓存。
        /// </summary>
        /// <typeparam name="TUser">用户类型的泛型参数。</typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="userId">要获取的用户 Id。</param>
        /// <param name="userFactory">用于创建用户的委托。</param>
        /// <param name="timeoutIfAdd">缓存超时时间。</param>
        /// <param name="useSlidingExpiration">是否使用滑动时间。</param>
        /// <returns></returns>
        public static TUser GetOrAddUser<TUser>(this ICacheManager cacheManager, long userId, Func<String, TUser> userFactory, TimeSpan? timeoutIfAdd = null, bool useSlidingExpiration = false)
            where TUser : UserBase
        {
            return cacheManager.GetOrSet(CacheKeyContracts.GetIdentityCacheKey(userId), userFactory, timeoutIfAdd, String.Empty, useSlidingExpiration);
        }

        /// <summary>
        /// 异步从缓存中获取用户数据。如果缓存不存在用户数据，使用 <paramref name="userFactory"/> 提供的回调方法创建一个用户并添加到缓存。
        /// </summary>
        /// <typeparam name="TUser">用户类型的泛型参数。</typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="userId">要获取的用户 Id。</param>
        /// <param name="userFactory">用于创建用户的委托。</param>
        /// <param name="timeoutIfAdd">缓存超时时间。</param>
        /// <param name="useSlidingExpiration">是否使用滑动时间。</param>
        /// <returns></returns>
        public static Task<TUser> GetOrAddUserAsync<TUser>(this ICacheManager cacheManager, long userId, Func<String, Task<TUser>> userFactory, TimeSpan? timeoutIfAdd = null, bool useSlidingExpiration = false)
            where TUser : UserBase
        {
            return cacheManager.GetOrSetAsync(CacheKeyContracts.GetIdentityCacheKey(userId), userFactory, timeoutIfAdd, String.Empty, useSlidingExpiration);
        }

        /// <summary>
        /// 从缓存中移除特定 Id 的用户数据。
        /// </summary>
        /// <param name="cacheManager"></param>
        /// <param name="userId">用户 Id。</param>
        public static void RemoveUser(this ICacheManager cacheManager, long userId)
        {
            cacheManager.Remove(CacheKeyContracts.GetIdentityCacheKey(userId));
        }
    }
}
