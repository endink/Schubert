using Microsoft.Extensions.Options;
using Schubert.Framework.Domain;
using Schubert.Framework.Environment;
using Schubert.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace Schubert.Framework.Environment
{
    public class LanguageStateProvider : IWorkContextStateProvider
    {
        private ILanguageService _languageService;
        private IOptions<SchubertOptions> _options;

        public LanguageStateProvider(ILanguageService languageService, 
            IOptions<SchubertOptions> options)
        {
            Guard.ArgumentNotNull(languageService, nameof(languageService));
            Guard.ArgumentNotNull(options, nameof(options));
            
            _languageService = languageService;
            _options = options;
        }

        public Func<WorkContext, Object> Get(string name)
        {
            if (name == WorkContext.CurrentLanguageStateName)
            {
                return (WorkContext ctx) => 
                {
                    string culture = ctx.CurrentUser?.Language?.IfNullOrWhiteSpace(_options.Value.DefaultCulture);
                    object lang = _languageService.GetLanguageAsync(culture).GetAwaiter().GetResult() ?? SchubertUtility.CreateLanguage(culture);
                    return lang;
                };
            }
            return null;
        }
    }
}
