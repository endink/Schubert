using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public static class PhysicalFileStorageExtensions
    {
        /// <summary>
        /// 以指定的配置添加物理文件存储器。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="options">物理文件存储配置。</param>
        public static void AddPhysicalFileStorage(this IFileStorageManager manager, PhysicalFileStorageOptions options)
        {
            Guard.ArgumentNotNull(options, nameof(options));

            if (options.FileMapping == null)
            {
                throw new ArgumentException($"必须为提供 {nameof(PhysicalFileStorageOptions)} 的非空 {nameof(PhysicalFileStorageOptions.FileMapping)} 属性。");
            }

            options = options ?? new PhysicalFileStorageOptions();
            PhysicalFileStorageProvider provider = new PhysicalFileStorageProvider(options);
            manager.AddProvider(provider);
        }

        /// <summary>
        /// 以指定的根目录为特定范围添加物理文件存储器。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="basePath">物理文件存储的根目录。</param>
        /// <param name="scopes">文件存储范围。</param>
        public static void AddPhysicalFileStorage(this IFileStorageManager manager, string basePath, params string[] scopes)
        {
            Guard.AbsolutePhysicalPath(basePath, nameof(basePath));
            var options = new PhysicalFileStorageOptions() { IncludeScopes = scopes, FileMapping = new DefaultFileRequestMapping(basePath) };
            PhysicalFileStorageProvider provider = new PhysicalFileStorageProvider(options);
            manager.AddProvider(provider);
        }

        /// <summary>
        /// 使用物理文件存储器用作临时文件存储。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="mapping">网络访问路径和物理路径的映射程序。</param>
        /// <param name="tempFileExpiredMinutes">临时文件过期时间（分钟）。</param>
        public static void UsePhysicalTemporaryFileStorage(this IFileStorageManager manager, IFileRequestMapping mapping, int tempFileExpiredMinutes = 30)
        {
            Guard.ArgumentNotNull(mapping, nameof(mapping));

            var provider = new PhysicalTemporaryFileStorageProvider(mapping,
                tempFileExpiredMinutes);
            manager.SetTemporaryProvider(provider);
        }

        /// <summary>
        /// 以指定的根目录和 URL 映射程序配置物理文件存储器用作临时文件存储。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="basePath">临时文件存储的根目录。</param>
        /// <param name="requestUrl">请求的根目录。</param>
        /// <param name="tempFileExpiredMinutes">临时文件过期时间（分钟）。</param>
        public static void UsePhysicalTemporaryFileStorage(this IFileStorageManager manager, string basePath, string requestUrl, int tempFileExpiredMinutes = 30)
        {
            Guard.AbsolutePhysicalPath(basePath, basePath);
            manager.UsePhysicalTemporaryFileStorage(new DefaultFileRequestMapping(basePath, requestUrl), tempFileExpiredMinutes);
        }
    }
}
