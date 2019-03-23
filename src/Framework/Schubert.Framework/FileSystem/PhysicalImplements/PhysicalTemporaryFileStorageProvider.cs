using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class PhysicalTemporaryFileStorageProvider : ITemporaryFileStorageProvider
    {
        private int _timeout = 0;
        private IFileRequestMapping _fileMapping;

        public PhysicalTemporaryFileStorageProvider(IFileRequestMapping fileMapping, int fileExpired = 30)
        {
            Guard.ArgumentNotNull(fileMapping, nameof(fileMapping));
            _timeout = fileExpired;
            _fileMapping = fileMapping;
        }

        public ITemporaryFileStorage CreateStorage()
        {
            return new PhysicalTemporaryFileStorage(_timeout, _fileMapping);
        }
    }
}
