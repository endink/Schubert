using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Caching
{
    /// <summary>
    /// 提供 Redis 序列化器名称常量。
    /// </summary>
    public static class RedisSerializerNames
    {
        /// <summary>
        /// Json.Net 序列化提供程序。
        /// </summary>
        public const string JsonNet = "sf.jnt";
    }
}
