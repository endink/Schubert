using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public interface IFile
    {
        /// <summary>
        /// 获取文件名。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取文件的完整路径。
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// 获取一个值，指示文件是否存在。
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// 获取文件最后更改时间（UTC）。
        /// </summary>
        DateTime LastModifiedTimeUtc { get; }

        /// <summary>
        /// 获取文件大小（单位 ：字节）
        /// </summary>
        long Length { get; }

        /// <summary>
        /// 获取文件访问 Url。
        /// </summary>
        /// <returns></returns>
        string CreateAccessUrl();
        
        /// <summary>
        /// 创建包含文件内容的流。
        /// </summary>
        /// <returns><see cref="Stream"/> 实例。</returns>
        Stream CreateReadStream();

        /// <summary>
        /// 异步创建包含文件内容的流。
        /// </summary>
        /// <returns><see cref="Stream"/> 实例。</returns>
        Task<Stream> CreateReadStreamAsync();
    }
}
