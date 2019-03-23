using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Schubert.Framework.Logging
{
    internal class FileLoggerOptionsSetup : ConfigureFromConfigurationOptions<FileLoggerOptions>
    {
        public FileLoggerOptionsSetup(IConfiguration providerConfiguration)
            : base(providerConfiguration.GetSection("Logging:File"))
        {
            
        }
    }
}
