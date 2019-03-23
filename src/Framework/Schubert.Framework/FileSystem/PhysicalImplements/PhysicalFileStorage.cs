using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class PhysicalFileStorage : IFileStorage
    {
        private IFileRequestMapping _fileMapping = null;
        private string _scope = null;

        public PhysicalFileStorage(string scope, IFileRequestMapping fileMapping)
        {
            Guard.ArgumentNotNull(fileMapping, nameof(fileMapping));
            _fileMapping = fileMapping;
            _scope = scope.IfNullOrWhiteSpace("DefaultRoot");
        }

        protected IFileRequestMapping Mapping
        {
            get { return _fileMapping; }
        }

        public async Task<IFile> CopyFromAsync(IFile file, string targetPath)
        {
            Guard.ArgumentNotNull(file, nameof(file));
            if (!file.Exists)
            {
                throw new ArgumentException("要从中复制内容的源文件不存在。", nameof(file));
            }
            Guard.ArgumentIsRelativePath(targetPath, nameof(targetPath));
            string fullPath = _fileMapping.GetFilePath(targetPath, _scope);
            using (var fs = await file.CreateReadStreamAsync())
            {
                return await CreateFileAsync(fullPath, fs);
            }
        }

        public IFileCreation CreateFile(string path)
        {
            Guard.ArgumentIsRelativePath(path, nameof(path));
            return new PhysicalFileCreation(_scope, path, _fileMapping);
        }

        public async Task<IFile> CreateFileAsync(string path, Stream streamInput)
        {
            Guard.ArgumentIsRelativePath(path, nameof(path));
            Guard.ArgumentNotNull(streamInput, nameof(streamInput));
            string fullPath = _fileMapping.GetFilePath(path, _scope);
            byte[] buffer = new byte[4096];
            int bytes;
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using (var ws = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                while ((bytes = await streamInput.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await ws.WriteAsync(buffer, 0, bytes);
                }
            }
            return new PhysicalFile(_scope, path, _fileMapping);
        }

        public Task<bool> DeleteFileAsync(string path)
        {
            Guard.ArgumentIsRelativePath(path, nameof(path));
            string fullPath = _fileMapping.GetFilePath(path, _scope);
            bool exisit = File.Exists(fullPath);
            File.Delete(fullPath);
            return Task.FromResult(exisit);
        }

        public Task<IFile> GetFileAsync(string path)
        {
            Guard.ArgumentIsRelativePath(path, nameof(path));
            return Task.FromResult<IFile>(new PhysicalFile(_scope, path, _fileMapping));
        }

        public IFileLock GetFileLock(string lockFilePath)
        {
            Guard.ArgumentIsRelativePath(lockFilePath, nameof(lockFilePath));
            string fullPath = _fileMapping.GetFilePath(lockFilePath, _scope);
            return new PhysicalFileLock(fullPath);
        }

        public Task<IEnumerable<IFile>> GetFilesAsync(string path = null, SearchOption searchOptions = SearchOption.TopDirectoryOnly)
        {
            IEnumerable<IFile> ifiles = Enumerable.Empty<IFile>();
            string fullPath = _fileMapping.GetFilePath(path, _scope);

            if (Directory.Exists(fullPath))
            {
                var files = Directory.EnumerateFiles(fullPath, "*.*", searchOptions);
                ifiles = files.Select(f => 
                {
                    string relative = _fileMapping.GetRelativeApplicationPath(f, _scope);
                    return new PhysicalFile(_scope, relative, _fileMapping);
                }).ToArray();
            }
            if (File.Exists(fullPath))
            {
                ifiles = new IFile[] { new PhysicalFile(_scope, path, _fileMapping) };
            }
            return Task.FromResult(ifiles);
        }
    }
}
