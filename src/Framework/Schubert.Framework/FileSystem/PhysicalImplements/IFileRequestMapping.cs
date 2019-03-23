using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    /// <summary>
    /// 对网络请求的文件路径映射到物理存储的提供程序。
    /// </summary>
    public interface IFileRequestMapping
    {
        /// <summary>
        /// 将应用程序相对路径（例如 AAA/bbb.jpg） 映射到物理绝对路径（例如 C:\AAA\bbb.jpg）
        /// </summary>
        /// <param name="relativePath">要映射的应用程序相对路径。</param>
        /// <param name="scope">存储区域。</param>
        /// <returns></returns>
        string GetFilePath(string relativePath, string scope);

        /// <summary>
        /// 将物理据对路径（例如 C:\AAA\bbb.jpg）映射为应用程序相对路径（例如 AAA/bbb.jpg） 
        /// </summary>
        /// <param name="physicalPath">要映射的物理路径。</param>
        /// <param name="scope">存储区域。</param>
        /// <returns></returns>
        string GetRelativeApplicationPath(string physicalPath, string scope);

        /// <summary>
        /// 根据文件完整路径获取网络访问的访问 Url。
        /// </summary>
        /// <param name="physicalPath">物理文件完整访问路径。</param>
        /// <returns>网络访问文件的地址。</returns>
        string CreateAccessUrl(string physicalPath);
    }
}
