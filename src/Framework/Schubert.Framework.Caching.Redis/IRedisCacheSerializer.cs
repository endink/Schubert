using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Caching
{
    /// <summary>
    /// 为 Redis 存储提供序列化和反序列化功能（框架默认使用 Json.Net）。
    /// </summary>
    public interface IRedisCacheSerializer
    {
        /// <summary>
        /// 获取序列化器的名称。
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 将 Redis 存储数据反序列化为强类型对象。
        /// </summary>
        /// <param name="reader">包含缓存数据的读取器。</param>
        /// <param name="objectType">对象类型</param>
        /// <returns></returns>
        object Deserialize(TextReader reader, Type objectType);
        /// <summary>
        /// 将要存储的数据序列化为流。
        /// </summary>
        /// <param name="textWriter">要写入数据的流编写器。</param>
        /// <param name="value">要写入的值</param>
        /// <returns></returns>
        void Serialize(TextWriter textWriter, object value);
    }
}
