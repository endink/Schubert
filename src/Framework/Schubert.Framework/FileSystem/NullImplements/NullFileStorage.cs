using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    internal class NullFileStorage : IFileStorage
    {
        public Task<IFile> CopyFromAsync(IFile file, string targetPath)
        {
            return Task.FromResult(NullFile.Create(targetPath));
        }

        public IFileCreation CreateFile(string path)
        {
            return new NullFileCreation(path);
        }

        public Task<IFile> CreateFileAsync(string path, Stream streamInput)
        {
            return Task.FromResult(NullFile.Create(path));
        }

        public Task<bool> DeleteFileAsync(string path)
        {
            return Task.FromResult(false);
        }

        public Task<IFile> GetFileAsync(string path)
        {
            return Task.FromResult(NullFile.Create(path));
        }

        public IFileLock GetFileLock(string lockFilePath)
        {
            return new NullFileLock();
        }

        public Task<IEnumerable<IFile>> GetFilesAsync(string path, SearchOption searchOption)
        {
            return Task.FromResult(Enumerable.Empty<IFile>());
        }
    }
}
