using Microsoft.AspNetCore.DataProtection.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Internal;
using System.IO;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Schubert.Framework.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Schubert.Framework.Web.DataProtection
{
    /// <summary>
    /// 提供文件存储的基础接口。
    /// </summary>
    public class FileStorageXmlRepository : IXmlRepository
    {
        private readonly Lazy<IFileStorage> _defaultStorageLazy = null;

        private readonly ILogger _logger;
        private string _pathBase;

        /// <summary>
        /// 创建用于存储密钥的 <see cref="FileStorageXmlRepository"/> 实例。
        /// </summary>
        /// <param name="services">用于提供服务的容器。</param>
        /// <param name="scope">存储区域，为空表示使用默认存储区域。</param>
        /// <param name="relativePath">文件夹路径，如果根目录请保持为空。</param>
        public FileStorageXmlRepository(IServiceProvider services, string scope = null, string relativePath = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            this.StorageScope = scope;
            Services = services;
            this._pathBase = relativePath?.Replace('\\', '/').Trim('/');
            _logger = services?.GetRequiredService<ILoggerFactory>().CreateLogger<FileSystemXmlRepository>();
            this._defaultStorageLazy = new Lazy<IFileStorage>(GetDefaultKeyStorage);
        }
        /// <summary>
        /// 获取提供服务的 <see cref="IServiceProvider"/> 服务容器。
        /// </summary>
        protected IServiceProvider Services { get; }

        /// <summary>
        /// 获取必要存储的区域。
        /// </summary>
        protected string StorageScope { get; }

        public virtual IReadOnlyCollection<XElement> GetAllElements()
        {
            // forces complete enumeration
            return GetAllElementsCore().ToList().AsReadOnly();
        }

        private IEnumerable<XElement> GetAllElementsCore()
        {
            var files = this._defaultStorageLazy.Value.GetFilesAsync(this._pathBase).GetAwaiter().GetResult();

            foreach (var file in files.Where(f=>!f.Name.EndsWith(".sf.lock", StringComparison.OrdinalIgnoreCase)))
            {
                XElement ele = ReadElementFromFile(file);
                if (ele != null)
                {
                   yield return ele;
                }
            }
        }

        private IFileStorage GetDefaultKeyStorage()
        {
            var manager = this.Services.GetRequiredService<IFileStorageManager>();
            return manager.CreateStorage(this.StorageScope);
        }

        private static bool IsSafeFilename(string filename)
        {
            // Must be non-empty and contain only a-zA-Z0-9, hyphen, and underscore.
            return (!String.IsNullOrWhiteSpace(filename) && filename.All(c =>
                c == '-'
                || c == '_'
                || ('0' <= c && c <= '9')
                || ('A' <= c && c <= 'Z')
                || ('a' <= c && c <= 'z')));
        }

        private XElement ReadElementFromFile(IFile file)
        {
            _logger.WriteDebug($"读取文件。路径：{file.FullPath}");

            using (var fileStream = file.CreateReadStream())
            {
                try
                {
                    return XElement.Load(fileStream);
                }
                catch (System.Xml.XmlException ex)
                {
                    this._logger?.WriteError($"读取 Asp Key 文件 {file.FullPath} 发生错误。", ex);
                    return null;
                }
            }
        }

        public virtual void StoreElement(XElement element, string friendlyName)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (!IsSafeFilename(friendlyName))
            {
                string newFriendlyName = Guid.NewGuid().ToString();
                _logger?.WriteWarning($"提供的文件名不是安全文件名（{friendlyName}），已变更为随机文件名。");
                friendlyName = newFriendlyName;
            }

            StoreElementCore(element, friendlyName);
        }

        private void StoreElementCore(XElement element, string filename)
        {
            string finalFilename = String.Concat(this._pathBase.IfNullOrWhiteSpace(String.Empty).TrimEnd('/'), _pathBase.IsNullOrWhiteSpace() ? String.Empty : "/", filename?.TrimStart('/'));
            using (var creation = _defaultStorageLazy.Value.CreateFile(finalFilename))
            {
                using (var locker = this._defaultStorageLazy.Value.GetFileLock($"{finalFilename}.sf.lock"))
                {
                    if (locker.TryLock(TimeSpan.FromMinutes(5)))
                    {
                        using (var stream = creation.OpenWriteStreamAsync().GetAwaiter().GetResult())
                        {
                            element.Save(stream);
                        }
                        creation.SaveChangesAsync().GetAwaiter().GetResult();
                        _logger.WriteDebug($"已写入 Key 文件（{filename}）。");
                    }
                    else
                    {
                        throw new SchubertException("写入 key 文件时候等待锁超时。");
                    }
                }
            }
        }
    }
}
