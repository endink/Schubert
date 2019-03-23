using System;
using System.Collections.Generic;
using System.Text;

namespace Schubert.Framework.Logging
{
    /// <summary>
    /// 文件日志选项。
    /// </summary>
    public class FileLoggerOptions
    {
        /// <summary>
        /// 获取或设置日志文件的保存目录（默认为当前目录下的 logs 目录）。
        /// </summary>
        public String Folder { get; set; } = "logs";

        /// <summary>
        /// 设置日志在内存中挤压的大小（单位 ：KB），默认为 20 M。
        /// </summary>
        public int BacklogSizeKB { get; set; } = 20 * 1024;
    }
}
