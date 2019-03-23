using Microsoft.Extensions.Logging;
using Schubert.Framework.Domain;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Schubert.Framework.Environment;
using System.Reflection;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Schubert.Framework.Logging
{
    /// <summary>
    /// 日志文件记录提供程序（默认文件将被记录在 ~/App_Data/{filename} 文件中）。
    /// </summary>
    [DebuggerDisplay("FileLogger : {FolderName}")]
    public class FileLogger : LoggerBase, IDisposable
    {
        private readonly string _folderName;
        private static readonly object _lock = new object();
        private static ConcurrentDictionary<String, BlockingContent> _blockingContents = null;
        private BlockingContent _currentContent = null;


        static FileLogger()
        {
            _blockingContents = new ConcurrentDictionary<string, BlockingContent>();
        }

        public FileLogger(string name, Func<string, LogLevel, bool> filter, string folder = null, int backlogBytesSize = 20 * 1024)
            : base(name, filter)
        {
            string currentFolder = SchubertUtility.GetApplicationDirectory();
            if (folder.IsNullOrWhiteSpace())
            {
                _folderName = currentFolder;
            }
            else
            {
                if (Path.IsPathRooted(folder))
                {
                    _folderName = folder;
                }
                else
                {
                    string dir = String.Concat(currentFolder.TrimEnd('\\', '/'), "/");
                    _folderName = new Uri(new Uri(dir), folder).LocalPath;
                }
            }
            if (!Directory.Exists(_folderName))
            {
                lock (_lock)
                {
                    if (!Directory.Exists(_folderName))
                    {
                        Directory.CreateDirectory(_folderName);
                    }
                }
            }
            BlockingContent content = null;
            _currentContent = _blockingContents.GetOrAdd(_folderName.ToLower(), k => (content = new BlockingContent(_folderName, backlogBytesSize)));
            if (content != null)
            {
                if (!Object.ReferenceEquals(_currentContent, content))
                {
                    content.Dispose();
                }
                else
                {
                    content.Run();
                }
            }
        }
        
        public string FolderName => _folderName;

        protected override void WriteLog(string name, EventId eventId, LogLevel level, string message, IEnumerable<KeyValuePair<string, object>> extensions)
        {
            _currentContent.WriteContent(message);
        }

        ~FileLogger()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disoposing)
        {
            if (disoposing)
            {
                if (_currentContent != null)
                {
                    _currentContent = null;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        [DebuggerDisplay("{CurrentFile}, backlog: {QueueSizeKBytes}")]
        private class BlockingContent : BlockConsumer
        {
            private readonly object SyncRoot = new object();
            private string _currentFile = null;
            private StreamWriter _writer = null;
            private string _folder = null;
            private ReaderWriterLockSlim _resetEvent;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="folder"></param>
            /// <param name="backlogSizeKB">允许内存中挤压的日志内容大小（默认为 20M）</param>
            public BlockingContent(string folder, int backlogSizeKB = 20 * 1024)
            {
                Guard.AbsolutePhysicalPath(folder, nameof(folder));
                this.QueueOptions = new QueueOptions() { ConsumeConcurrencyLevel = 1, QueueMaxSizeKBytes = backlogSizeKB };
                _folder = folder;
                _resetEvent = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            }

            public string Folder { get => _folder; }
            public string CurrentFile { get => _currentFile; }

            protected override QueueOptions QueueOptions { get; }

            public void WriteContent(String content)
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                this.EqueueBuffer(bytes, "0");
            }

            protected override void Consume(string key, byte[] dataBytes, byte[] rawBytes)
            {
                string fileName = $"{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                if (_currentFile.IsNullOrWhiteSpace() || !_currentFile.Equals(fileName))
                {
                    lock (SyncRoot)
                    {
                        if (_currentFile.IsNullOrWhiteSpace() || !_currentFile.Equals(fileName))
                        {
                            ResetWriter(fileName);
                        }
                    }
                }
                WriteContent(dataBytes);
            }

            private void WriteContent(byte[] dataBytes)
            {
                _resetEvent.EnterReadLock();
                try
                {
                    _writer.WriteLine(Encoding.UTF8.GetString(dataBytes));
                    _writer.Flush();
                }
                finally
                {
                    _resetEvent.ExitReadLock();
                }
            }

            private void ResetWriter(string fileName)
            {
                _resetEvent.EnterWriteLock();
                try
                {
                    _currentFile = fileName;
                    _writer?.Dispose();
                    _writer = File.CreateText(Path.Combine(_folder, fileName));
                }
                finally
                {
                    _resetEvent.ExitWriteLock();
                }
            }

            protected override void OnDisposing()
            {
                base.OnDisposing();
                _writer?.Dispose();
                _writer = null;
                _resetEvent?.Dispose();
                _resetEvent = null;
            }
        }
    }
}
