using Microsoft.Extensions.Options;
using Schubert.Framework.Environment;
using Schubert.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    public class TimeZoneStateProvider : IWorkContextStateProvider
    {
        private IOptions<SchubertOptions> _options;
        public TimeZoneStateProvider(IOptions<SchubertOptions> configOptions)
        {
            Guard.ArgumentNotNull(configOptions, nameof(configOptions));
            _options = configOptions;
        }

        public Func<WorkContext, object> Get(string name)
        {
            if (name == WorkContext.CurrentTimeZoneState)
            {
                return (WorkContext ctx) =>
                {
                    String timeZone = ctx.CurrentUser?.TimeZone;
                    string timeZoneId = timeZone.IfNullOrWhiteSpace(_options.Value.DefaultTimeZone);
                    return TimeZoneHelper.GetTimeZoneInfo(timeZoneId) ?? TimeZoneInfo.Local;
                };
            }
            return null;
        }
    }
}
