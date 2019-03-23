using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class PhysicalTemporaryFileStorage : PhysicalFileStorage, ITemporaryFileStorage
    {
        public const string TempScope = "__temp";
        private int _tempTimeoutMinutes = 0;
        private string _directory = null;

        public PhysicalTemporaryFileStorage(int tempTimeoutMinutes, IFileRequestMapping fileMapping)
            :base(TempScope,fileMapping)
        {
            _tempTimeoutMinutes = Math.Max(1, _tempTimeoutMinutes);
        }

        public Task ClearAsync()
        {
            return Task.Run(() => {
                _directory = this.Mapping.GetFilePath("/", TempScope);
                if (Directory.Exists(_directory))
                {
                    var files = Directory.EnumerateFiles(_directory, "*", SearchOption.AllDirectories);
                    Parallel.ForEach(files, f => {
                        File.Delete(f);
                    });
                }
            });
        }
    }
}
