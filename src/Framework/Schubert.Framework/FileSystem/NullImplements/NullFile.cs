using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public sealed class NullFile : IFile
    {
        private string _name;
        private string _path;

        public static IFile Create(string path = null)
        {
            return new NullFile(path);
        }

        private NullFile(string path)
        {
            Guard.ArgumentNotNull(path, nameof(path));
            this._path = path.IfNullOrWhiteSpace(String.Empty);
            this._name = path.IsNullOrWhiteSpace() ? String.Empty : Path.GetFileName(path);
        }
        

        bool IFile.Exists { get { return false; } }

        string IFile.FullPath { get { return _path; } }

        DateTime IFile.LastModifiedTimeUtc { get { return DateTimeOffset.MinValue.UtcDateTime; } }

        long IFile.Length { get { return 0; } }

        string IFile.Name { get { return _name; } }

        string IFile.CreateAccessUrl()
        {
            return String.Empty;
        }

        Stream IFile.CreateReadStream()
        {
            return null;
        }

        Task<Stream> IFile.CreateReadStreamAsync()
        {
            return Task.FromResult((Stream)null);
        }
    }
}
