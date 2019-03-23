using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Localization
{
    public class SchubertStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly IResourceNamesCache _resourceNamesCache = new ResourceNamesCache();
        private ILocalizedStringManager _localizedStringManager = null;

        public SchubertStringLocalizerFactory(ILocalizedStringManager localizedStringManager)
        {
            Guard.ArgumentNotNull(localizedStringManager, nameof(localizedStringManager));

            _localizedStringManager = localizedStringManager;
        }
        public IStringLocalizer Create(Type resourceSource)
        {
            return new SchubertStringLocalizer(_resourceNamesCache, _localizedStringManager);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return new SchubertStringLocalizer(_resourceNamesCache, _localizedStringManager);
        }
    }
}
