using Schubert.Framework.FileSystem.AppData;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Schubert.Framework.Services
{
    public class DebugOnlyFileStoreService : IDistributedOptimisticStoreService
    {
        public string FirstCreationData { get; set; }

        protected Func<IAppDataFolder> _dataFolderFactory = null;
        
        public DebugOnlyFileStoreService(Func<IAppDataFolder> factory)
        {
            _dataFolderFactory = factory;
        }

        public async Task<String> GetDataAsync(string blockName)
        {
            IAppDataFolder folder = _dataFolderFactory();
            string file = $@"DebugDistributedStore/{blockName}.txt";
            if (folder.FileExists(file))
            {
                string text = File.ReadAllText(file);
                return await Task.FromResult(text);
            }
            else
            {
                using (var stream = File.Create(file))
                {
                    using (var streamWriter = new StreamWriter(stream))
                    {
                        streamWriter.Write(this.FirstCreationData);
                    }
                }
                return await Task.FromResult(this.FirstCreationData);
            }
        }

        public async Task<bool> TryWriteDataAsync(string blockName, string data)
        {
            IAppDataFolder folder = _dataFolderFactory();
            string file = $@"DebugDistributedStore/{blockName}.txt";
            using (Stream stream = folder.OpenFile(file))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    streamWriter.Write(data);
                }
            }

            return await Task.FromResult(true);
        }
    }
}
