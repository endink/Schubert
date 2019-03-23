using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    /// <summary>
    /// 表示一个文件创建的操作（用于提供文件创建的流）。
    /// </summary>
    public interface IFileCreation : IDisposable
    {
        /// <summary>
        /// 打开用于写入文件内容的流。
        /// </summary>
        Task<Stream> OpenWriteStreamAsync();

        /// <summary>
        /// 保存在留中写入的更改。
        /// </summary>
        /// <returns><see cref="IFile"/> 对象。</returns>
        Task<IFile> SaveChangesAsync();
    }
}
