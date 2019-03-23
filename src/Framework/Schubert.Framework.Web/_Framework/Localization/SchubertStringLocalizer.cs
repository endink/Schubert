using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace Schubert.Framework.Localization
{
    public class SchubertStringLocalizer : IStringLocalizer
    {
        private ILocalizedStringManager _localizedStringManager = null;
        private IResourceNamesCache _resourceNamesCache = null;

        public SchubertStringLocalizer(
            IResourceNamesCache resourceNamesCache,
            ILocalizedStringManager localizedStringManager)
        {
            Guard.ArgumentNotNull(resourceNamesCache, nameof(resourceNamesCache));
            Guard.ArgumentNotNull(localizedStringManager, nameof(localizedStringManager));

            _resourceNamesCache = resourceNamesCache;
            _localizedStringManager = localizedStringManager;
        }

        #region private

        private IEnumerable<string> GetResourceNamesFromCultureHierarchy(CultureInfo startingCulture)
        {
            var currentCulture = startingCulture;
            var resourceNames = new HashSet<string>();

            while (true)
            {
                try
                {
                    IList<string> cultureResourceNames = GetResourceNamesFromCulture(currentCulture);

                    foreach (var resourceName in cultureResourceNames)
                    {
                        resourceNames.Add(resourceName);
                    }
                }
                catch (SchubertException) { }

                if (currentCulture == currentCulture.Parent)
                {
                    // currentCulture begat currentCulture, probably time to leave
                    break;
                }

                currentCulture = currentCulture.Parent;
            }

            return resourceNames;
        }

        private IList<string> GetResourceNamesFromCulture(CultureInfo currentCulture)
        {
            return _resourceNamesCache.GetOrAdd(
                currentCulture.Name,
                n => _localizedStringManager.GetLocalizedStringKeys(n).GetAwaiter().GetResult().ToList());
        }


        #endregion


        public LocalizedString this[string name]
        {
            get
            {
                var str = _localizedStringManager.GetLocalizedString(CultureInfo.CurrentUICulture.Name, name);
                return new LocalizedString(name, str ?? name, str != null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var format = _localizedStringManager.GetLocalizedString(CultureInfo.CurrentUICulture.Name, name);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, format == null);

            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
        {
            var resourceNames = includeAncestorCultures
                ? GetResourceNamesFromCultureHierarchy(CultureInfo.CurrentUICulture)
                : GetResourceNamesFromCulture(CultureInfo.CurrentUICulture);

            foreach (var name in resourceNames)
            {
                var value = _localizedStringManager.GetLocalizedString(CultureInfo.CurrentUICulture.Name, name);
                yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }

        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            culture = culture ?? CultureInfo.CurrentUICulture;
            return new SchubertStringLocalizer(_resourceNamesCache, _localizedStringManager);
        }


    }
}
