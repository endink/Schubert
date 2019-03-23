using Microsoft.Extensions.Options;
using Swifty.MicroServices.Client;
using Swifty.Services;
using System;
using System.Collections.Generic;
using Swifty;

namespace Schubert.Framework.Swifty
{
    public sealed class SwiftyClientManager
    {
        private IOptions<SwiftyOptions> _options;
        private SwiftyClient _innerClient;
        private readonly Object _syncRoot = new object();

        public SwiftyClientManager(IOptions<SwiftyOptions> options)
        {
            Guard.ArgumentNotNull(options, nameof(options));
            if (!(options.Value?.EnableFeatures ?? SwiftyFeatures.Client).HasFlag(SwiftyFeatures.Client))
            {
                throw new SwiftyApplicationException($"{nameof(SwiftyOptions)} 配置为禁用 Client 功能，无法使用 {nameof(SwiftyClientManager)}，考虑更改 {nameof(SwiftyOptions)}.{nameof(SwiftyOptions.EnableFeatures)} 属性。");
            }
            _options = options;
        }

        public SwiftyClient Client
        {
            get
            {
                if (_innerClient == null)
                {
                    lock (_syncRoot)
                    {
                        if (_innerClient == null)
                        {
                            _innerClient = new SwiftyClient(_options.Value.Client ?? new SwiftyClientOptions());
                        }
                    }
                }
                return _innerClient;
            }
        }

    }
}
