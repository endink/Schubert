using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class PhysicalFileLock : IFileLock
    {
        private FileStream _lockStream = null;
        private string _filePath = null;
        private object _syncRoot = null;
        public PhysicalFileLock(string lockFilePath)
        {
            Guard.AbsolutePhysicalPath(lockFilePath, nameof(lockFilePath));
            _filePath = lockFilePath;
            _syncRoot = new object();
        }

        public bool IsLocked
        {
            get
            {
                return _lockStream != null;
            }
        }

        public void Dispose()
        {
            this.Release();
        }

        public void Release()
        {
            lock(_syncRoot)
            {
                _lockStream?.Dispose();
                _lockStream = null;
            }
        }

        public bool TryLock()
        {
            lock(_syncRoot)
            {
                try
                {
                    if (!File.Exists(_filePath))
                    {
                        string directory = Path.GetDirectoryName(_filePath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                        using (var fs = File.CreateText(_filePath))
                        {
                            fs.WriteLine(DateTime.UtcNow);
                        }
                    }
                    this._lockStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }
        }

        private bool TryLockLoop(double seconds)
        {
            bool locked = false;
            while(true)
            {
                locked = this.TryLock();
                if (locked || seconds <= 0)
                {
                    return locked;
                }
                Thread.Sleep(1000);
                this.TryLockLoop(seconds - 1d);
            }
        }

        public bool TryLock(TimeSpan lockPeriod)
        {
            double seconds = lockPeriod.TotalSeconds;
            return this.TryLockLoop(seconds);
        }
    }
}
