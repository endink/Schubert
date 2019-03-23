using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class PhysicalFile : IFile
    {
        private string _fullPath = null;
        private IFileRequestMapping _mapping = null;
        private string _url = null;
        private string _filePath = null;
        private string _fileName = null;
        public PhysicalFile(string scope, string filePath, IFileRequestMapping fileRequestMapping)
        {
            Guard.ArgumentNotNull(fileRequestMapping, nameof(fileRequestMapping));
            Guard.ArgumentIsRelativePath(filePath, nameof(filePath));
            _filePath = filePath;
            _fullPath = fileRequestMapping.GetFilePath(filePath, scope);
            _mapping = fileRequestMapping;
        }

        public bool Exists
        {
            get { return !this._fullPath.IsNullOrWhiteSpace() && File.Exists(this._fullPath); }
        }

        public string FullPath
        {
            get
            {
                return _filePath;
            }
        }

        public DateTime LastModifiedTimeUtc
        {
            get
            {
                return File.GetLastWriteTimeUtc(_fullPath);
            }
        }

        public long Length
        {
            get
            {
                FileInfo file = new FileInfo(_fullPath);
                return file.Length;
            }
        }

        public string Name
        {
            get
            {
                return (_fileName ?? (_fileName = Path.GetFileName(_fullPath)));
            }
        }

        public string CreateAccessUrl()
        {
            return _url ?? (_url = _mapping.CreateAccessUrl(_fullPath));
        }

        public Stream CreateReadStream()
        {
            return File.OpenRead(_fullPath);
        }

        public Task<Stream> CreateReadStreamAsync()
        {
            return Task.FromResult<Stream>(File.OpenRead(_fullPath));
        }
    }
}
