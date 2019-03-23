using System;

namespace Schubert.Framework.Environment
{
    /// <summary>
    /// 基础路径提供程序接口。
    /// </summary>
    public interface IPathProvider
    {
        /// <summary>
        /// 获取应用程序当前的根目录.
        /// </summary>
        string RootDirectoryPhysicalPath { get; }

        /// <summary>
        /// 将虚拟路径映射到应用程序的物理路径上。
        /// </summary>
        /// <param name="virtualPath">要映射的虚拟目录（例如：~/）。</param>
        /// <returns>指定的虚拟目录的完整物理路径。</returns>
        string MapApplicationPath(string virtualPath);

        //string GetPackagePath(string packageName);
    }
}