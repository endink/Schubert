using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    /// <summary>
    /// 表示一个临时文件存储提供程序的接口（除了具有普通文件存储功能外还具备清理功能）。
    /// </summary>
    public interface ITemporaryFileStorage : IFileStorage
    {
        /// <summary>
        /// 清理临时文件操作（通常情况下，考虑一个超时时间来清理临时文件）。
        /// </summary>
        Task ClearAsync();
    }
}
