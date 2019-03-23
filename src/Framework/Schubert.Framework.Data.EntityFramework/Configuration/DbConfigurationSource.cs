using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Configuration
{
    public class DbConfigurationSource : IConfigurationSource
    {
        private string _templateJsonFile = null;
        private string _region = null;
        public DbConfigurationSource(string templateJsonFile, string region)
        {
            _templateJsonFile = templateJsonFile.IfNullOrWhiteSpace(String.Empty).Replace('/', '\\');
            _region = region;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            string jsonContent = null;
            if (!_templateJsonFile.IsNullOrWhiteSpace())
            {
                var fp = builder.GetFileProvider();
                string rootDirectory = System.IO.Path.GetDirectoryName(fp.GetFileInfo("config.json").PhysicalPath);
                var file = System.IO.Path.Combine(rootDirectory, _templateJsonFile);
                file = Path.GetFullPath(file);
                if (File.Exists(file))
                {
                    jsonContent = File.ReadAllText(file, Encoding.UTF8);
                }
            }
            return new DbConfigurationProvider(_region, jsonContent);
        }
    }
}
