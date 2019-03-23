using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public interface IFileStorage
    {
        /// <summary>
        /// 枚举一个路径下的临时文件。
        /// </summary>
        /// <param name="path">相对路径 （例如 AA/BB，AA）。</param>
        /// <param name="searchOptions">指示是否递归搜索所有目录。</param>
        /// <returns>如果 <paramref name="path"/> 为目录将返回包含临时文件的集合；如果 <paramref name="path"/> 为文件集合中只含有一个实例。</returns>
        Task<IEnumerable<IFile>> GetFilesAsync(string path = null, SearchOption searchOptions = SearchOption.TopDirectoryOnly);

        /// <summary>
        /// 通过指定的相对路径获取零时文件。
        /// </summary>
        /// <param name="path">临时文件相对路径（例如:AA/BB/xxx.jpg）。</param>
        /// <returns><see cref="IFile" /> 对象，该返回值不会为空。</returns>
        Task<IFile> GetFileAsync(string path);
        /// <summary>
        /// 删除给定路径的临时文件。
        /// </summary>
        /// <param name="path">文件路径（如果路径不是文件，将不会进行任何操作）</param>
        /// <returns>返回一个值，指示是否删除了文件。</returns>
        Task<bool> DeleteFileAsync(string path);
        
        /// <summary>
        /// 删除给定路径的临时文件。
        /// </summary>
        /// <param name="path">文件路径（指定路径存在文件时将覆盖原有文件）。</param>
        /// <param name="streamInput"> 包含文件内容的输入流。</param>
        /// <returns>返回一个值，指示是否创建了文件。</returns>
        Task<IFile> CreateFileAsync(string path, Stream streamInput);

        /// <summary>
        /// 获取指定路径的文件创建操作对象。
        /// </summary>
        /// <param name="path">文件路径（指定路径存在文件时将覆盖原有文件）。</param>
        /// <returns>一个包含数据流的用于创建文件的对象。</returns>
        IFileCreation CreateFile(string path);

        /// <summary>
        /// 从给定的文件中拷贝内容。
        /// </summary>
        /// <param name="file">要从中拷贝内容的源文件。</param>
        /// <param name="targetPath">拷贝文件的目标路径。</param>
        Task<IFile> CopyFromAsync(IFile file, string targetPath);
        /// <summary>
        /// 获取文件锁。
        /// </summary>
        /// <param name="lockFilePath">锁文件的路径。</param>
        /// <returns></returns>
        IFileLock GetFileLock(string lockFilePath);

    }
    
}
