using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schubert.Framework.FileSystem.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Logging
{
    [ProviderAlias("File")]
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly string _folder;
        private int _backlogSize;

        private static readonly Func<string, LogLevel, bool> TrueFilter = (cat, level) => true;
        private static readonly Func<string, LogLevel, bool> FalseFilter = (cat, level) => false;

        public FileLoggerProvider(string folderName, Func<string, LogLevel, bool> filter= null, int backlogSize = 10 * 1024)
        {
            this._filter = filter ?? TrueFilter;
            this._folder = folderName.IfNullOrWhiteSpace("logs");
            this._backlogSize = Math.Max(4, backlogSize);
        }
        
        public FileLoggerProvider(IOptions<FileLoggerOptions> options)
        {
            Guard.ArgumentNotNull(options, nameof(options));
            _filter = TrueFilter;
            _folder = options.Value.Folder.IfNullOrWhiteSpace("logs");
            _backlogSize = Math.Max(4, options.Value.BacklogSizeKB);
        }

        public ILogger CreateLogger(string name)
        {
            return new FileLogger(name, _filter, _folder, _backlogSize);
        }

        public void Dispose()
        {
             
        }
    }
}
