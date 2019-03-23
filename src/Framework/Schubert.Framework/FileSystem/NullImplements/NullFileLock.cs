using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class NullFileLock : IFileLock
    {
        public bool IsLocked
        {
            get
            {
                return false;
            }
        }

        public void Dispose()
        {
        }

        public void Release()
        { }

        public bool TryLock()
        {
            return true;
        }

        public bool TryLock(TimeSpan lockWaitTimeout)
        {
            return true;
        }
    }
}
