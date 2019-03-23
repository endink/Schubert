using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class NullFileCreation : IFileCreation
    {
        private string _path;
        public NullFileCreation(string path)
        {
            _path = path ?? String.Empty;
        }

        public void Dispose()
        {
            
        }

        public Task<Stream> OpenWriteStreamAsync()
        {
            return Task.FromResult(Stream.Null);
        }

        public IFile SaveChanges()
        {
            return NullFile.Create(_path);
        }

        public Task<IFile> SaveChangesAsync()
        {
            IFile f = NullFile.Create(_path);
            return Task.FromResult(f);
        }
    }
}
