using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class FileStorageManager : IFileStorageManager
    {
        private HashSet<IFileStorageProvider> _providers = null;
        private ITemporaryFileStorageProvider _tempProvider = null;
        private Regex _scopeNameRegex;

        public FileStorageManager()
        {
            this._providers = new HashSet<IFileStorageProvider>();

            string validBlobContainerNameRegex = @"^([a-z]|\d){1}([a-z]|-|\d){1,61}([a-z]|\d){1}$";
            _scopeNameRegex = new Regex(validBlobContainerNameRegex);
        }
        
        public ITemporaryFileStorage Temporary
        {
            get { return this.CreateTemporaryStorage(); }
        }

        public ITemporaryFileStorage CreateTemporaryStorage()
        {
            return (_tempProvider?.CreateStorage() ?? new NullTemporaryFileStorage());
        }

        public void AddProvider(IFileStorageProvider provider)
        {
            _providers.Add(provider);
        }

        public IFileStorage CreateStorage(string scope = null)
        {
            if (!scope.IsNullOrWhiteSpace())
            {
                scope = scope.ToLower();
                if (!_scopeNameRegex.IsMatch(scope))
                {
                    throw new ArgumentOutOfRangeException(@"文件存储的 scope 名称长度为 3 ~ 63 个字符，必须以字母开头，并且只能包含字母、数字、""-"" 三种字符。");
                }
            }
            foreach (var p in this._providers)
            {
                var storage = p.CreateStorage(scope);
                if (storage != null)
                {
                    return storage;
                }
            }
            return new NullFileStorage();
        }

        public void SetTemporaryProvider(ITemporaryFileStorageProvider provider)
        {
            _tempProvider = provider;
        }
    }
}
