using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.FileProviders
{
    /// <summary>
    /// 存储一个应用程序目录（/XXXX） 和对应的一个物理目录（C:\XXXX）。
    /// </summary>
    public sealed class VirtualDirectoryMapping
    {
        /// <summary>
        /// 创建 <see cref="VirtualDirectoryMapping"/> 类的新实例。
        /// </summary>
        /// <param name="rootPhysicalDirectory">物理路径，rootPhysicalDirectory 必须提供根目录形式的目录名。</param>
        /// <param name="appPath">应用程序目录，例如 ~/ ，可以是根路径也可以是相对路径。</param>
        public VirtualDirectoryMapping(string rootPhysicalDirectory, string appPath)
        {
            Guard.ArgumentNullOrWhiteSpaceString(rootPhysicalDirectory, nameof(rootPhysicalDirectory));
            Guard.ArgumentNullOrWhiteSpaceString(appPath, nameof(appPath));

            Guard.ArgumentContainsInvalidPathChars(rootPhysicalDirectory, nameof(rootPhysicalDirectory));
            Guard.ArgumentContainsInvalidPathChars(appPath, nameof(appPath));

            if (!Path.IsPathRooted(rootPhysicalDirectory))
            {
                throw new ArgumentException($"参数 {nameof(rootPhysicalDirectory)} 必须是绝对物理路径。", nameof(rootPhysicalDirectory));
            }

            if (!Directory.Exists(rootPhysicalDirectory))
            {
                throw new ArgumentException($"参数 {nameof(rootPhysicalDirectory)} 必须是有效的目录路径，不能是文件。", nameof(rootPhysicalDirectory));
            }

            this.PhysicalDirectory = rootPhysicalDirectory;
            this.VirtualDirectory = appPath;


        }

        /// <summary>
        /// 获取虚拟目录信息。
        /// </summary>
        public string VirtualDirectory { get; private set; }

        /// <summary>
        /// 获取物理目录信息。
        /// </summary>
        public string PhysicalDirectory { get; private set; }
    }
}
