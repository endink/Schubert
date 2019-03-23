using System;

namespace Schubert.Framework.Caching
{
    /// <summary>
    /// 缓存管理接口，接口中的 region 只提供逻辑上的分区，key 必须全局唯一。
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// 根据指定的缓存键获取缓存实例。
        /// </summary> 
        /// <param name="key">缓存键。</param>
        /// <param name="region">缓存区域。</param>
        /// <returns>缓存键对应的缓存实例。为空表示缓存中不存在键为 key 的对象。</returns>
        object Get(string key, string region = "");

        /// <summary>
        /// 将对象以指定的缓键添加到缓存, 如果已存在键为 key 的缓存对象 ，则更新此对象。
        /// </summary>
        /// <param name="key">缓存键。</param>
        /// <param name="data">要添加到缓存的对象。</param>
        /// <param name="timeout">缓存过期时间， 为空表示永不过期。</param>
        /// <param name="region">缓存区域。</param>
        /// <param name="useSlidingExpiration">指示是否使用滑动时间（每次使用会刷新过期时间）过期策略。</param>
        void Set(string key, object data, TimeSpan? timeout = null, string region = "", bool useSlidingExpiration = false);

        /// <summary>
        /// 从缓存中移除指定键的缓存实例。
        /// </summary>
        /// <param name="key">要移除的缓存实例的键值。</param>
        /// <param name="region">缓存区域。</param>
        void Remove(string key, string region = "");

        /// <summary>
        /// 表示对滑动过期时间的缓存重新计时（绝对过期时间该操作无效）。
        /// </summary>
        /// <param name="key">要刷新的缓存实例的键值。</param>
        /// <param name="region">缓存区域。</param>
        /// <returns>返回一个布尔值，指示是否缓存对象被刷新（如果缓存中未找到对象会返回 null）。</returns>
        bool Refresh(string key, string region = "");

        /// <summary>
        /// 清空指定区域的缓存。
        /// </summary>
        void ClearRegion(string region = "");

        /// <summary>
        /// 清空所有缓存。
        /// </summary>
        void Clear();
    }
}