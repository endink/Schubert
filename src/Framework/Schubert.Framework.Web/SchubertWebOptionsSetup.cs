using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Schubert.Framework.Web
{
    public class SchubertWebOptionsSetup : ConfigureFromConfigurationOptions<SchubertWebOptions>
    {
        public SchubertWebOptionsSetup(IConfiguration configuration)
            :base(configuration)
        {

        }
    }
}
