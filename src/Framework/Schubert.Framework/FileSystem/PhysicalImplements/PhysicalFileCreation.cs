using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class PhysicalFileCreation : IFileCreation
    {
        private string _tempFilePath = null;
        private string _relativePath = null;
        private string _scope = null;
        private IFileRequestMapping _fileMapping = null;

        internal PhysicalFileCreation(string scope, string filePath,IFileRequestMapping fileRequestMapping)
        {
            Guard.ArgumentNotNull(fileRequestMapping, nameof(fileRequestMapping));
            Guard.ArgumentIsRelativePath(filePath, nameof(filePath));

            _fileMapping = fileRequestMapping;
            _scope = scope;
            _relativePath = filePath;
            _tempFilePath = Path.GetTempFileName();
        }

        public void Dispose()
        {
            File.Delete(_tempFilePath);
        }

        public Task<Stream> OpenWriteStreamAsync()
        {
            string directory = Path.GetDirectoryName(_tempFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            return Task.FromResult<Stream>(File.OpenWrite(_tempFilePath));
        }

        public Task<IFile> SaveChangesAsync()
        {
            string fullPath = _fileMapping.GetFilePath(_relativePath, _scope);
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.Copy(_tempFilePath, fullPath, true);
            return Task.FromResult<IFile>(new PhysicalFile(_scope, _relativePath, _fileMapping));
        }
    }
}
