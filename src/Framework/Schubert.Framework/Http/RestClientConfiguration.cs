using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class RestClientConfiguration
    {
        public Uri EndpointBaseUri { get; internal set; }

        public RestCredentials Credentials { get; internal set; }

        public TimeSpan Timeout { get; internal set; } = TimeSpan.FromSeconds(100);

        public Dictionary<String, String> DefaultHeaders { get; }

        public RestClientConfiguration(Uri endpoint, RestCredentials credentials = null,
            TimeSpan timeout = default(TimeSpan))
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));


            Credentials = credentials ?? new AnonymousCredentials();
            EndpointBaseUri = endpoint;
            if (timeout != TimeSpan.Zero)
            {
                if (timeout < System.Threading.Timeout.InfiniteTimeSpan)
                    // TODO: Should be a resource for localization.
                    // TODO: Is this a good message?
                    throw new ArgumentException("Timeout must be greater than System.Threading.Timeout.Infinite", nameof(timeout));
                Timeout = timeout;
            }
            this.DefaultHeaders = new Dictionary<string, string>();
        }
        
        public void Dispose()
        {
            Credentials.Dispose();
        }
    }
}
