using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public interface IFileStorageProvider
    {
        IFileStorage CreateStorage(string scope = null);
    }
}
