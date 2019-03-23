using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Caching
{
    public static class CacheKeyContracts
    {
        public const string AuthenticatedUserCacheKeyFormat = "sf-user-{0}";

        /// <summary>
        /// 根据指定的用户 Id 获取用户缓存键。
        /// </summary>
        public static string GetIdentityCacheKey(long userId)
        {
            return String.Format(AuthenticatedUserCacheKeyFormat, userId);
        }
    }
}
