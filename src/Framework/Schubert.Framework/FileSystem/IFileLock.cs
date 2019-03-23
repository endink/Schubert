using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    /// <summary>
    /// 表示一个文件锁。
    /// </summary>
    public interface IFileLock : IDisposable
    {
        /// <summary>
        /// 获取或设置该文件是否被锁定。
        /// </summary>
        bool IsLocked { get; }
        
        /// <summary>
        /// 尝试对文件加锁。
        /// </summary>
        /// <returns></returns>
        bool TryLock();

        /// <summary>
        /// 尝试对文件加锁。使用此方法在加锁不成功时只要未超时会进行重试。
        /// </summary>
        /// <param name="lockPeriod">加锁操作超时的时间。</param>
        /// <returns></returns>
        bool TryLock(TimeSpan lockPeriod);

        /// <summary>
        /// 使用文件上的锁。
        /// </summary>
        /// <returns></returns>
        void Release();
    }
}
