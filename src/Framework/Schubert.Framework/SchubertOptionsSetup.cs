using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Schubert.Framework
{
    public class SchubertOptionsSetup : ConfigureFromConfigurationOptions<SchubertOptions>
    {
        public SchubertOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {

        }
    }
}
