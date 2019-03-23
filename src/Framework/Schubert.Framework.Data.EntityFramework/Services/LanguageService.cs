using Microsoft.EntityFrameworkCore;
using Schubert.Framework.Caching;
using Schubert.Framework.Domain;
using Schubert.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data.Services
{
    public class LanguageService : ILanguageService
    {
        /// <summary>
        /// 语言缓存键 sf.lang.{0}，参数: <see cref="Language.Culture"/> 。
        /// </summary>
        public const string LanguageCacheKeyFormat = "sf.lang.{0}";

        private ICacheManager _cacheManager;

        private IRepository<StringResource> _stringResourceLanguageRepository;
        private IRepository<Language> _languageRepository;

        public LanguageService(
            ICacheManager cacheManager,
            IRepository<Language> languageRepository,
            IRepository<StringResource> stringResourceLanguageRepository)
        {
            Guard.ArgumentNotNull(cacheManager, nameof(cacheManager));
            Guard.ArgumentNotNull(languageRepository, nameof(languageRepository));
            Guard.ArgumentNotNull(stringResourceLanguageRepository, nameof(stringResourceLanguageRepository));

            _cacheManager = cacheManager;

            _languageRepository = languageRepository;
            _stringResourceLanguageRepository = stringResourceLanguageRepository;
        }

        public static string GetLanguageCacheKey(string culture)
        {
            return String.Format(LanguageCacheKeyFormat, culture.ToLower());
        }

        private async Task<bool> ExistingLanguageAsync(string culture)
        {
            bool exisiting = await _languageRepository.Table.AnyAsync(l => l.Culture == culture);
            return exisiting;
        }

        public async Task<int> AddStringResourcesAsync(IEnumerable<StringResource> resources, AdditionPolicy policy)
        {
            if (resources.IsNullOrEmpty())
            {
                return 0;
            }
            HashSet<String> namesForQuery = new HashSet<string>();
            foreach (var r in resources)
            {
                ThrowIfInvalidResource(r);
                namesForQuery.Add(r.Culture.ToLower());
            }
            var existingLanguages = await (from l in _languageRepository.Table
                             where namesForQuery.Contains(l.Culture)
                             select l)
                             .Include(l => l.StringResources).ToArrayAsync();

            List<Language> newLanguages = new List<Language>();
            int changedCount = 0;
            foreach (var r in resources)
            {
                Language resourceLang = existingLanguages.FirstOrDefault(l => l.Culture.CaseInsensitiveEquals(r.Culture));
                bool exisitngLang = resourceLang != null;
                if (exisitngLang)
                {
                    if (AddExsitingLanguageResource(r, policy, resourceLang))
                    {
                        changedCount++;
                    }
                }
                else
                {
                    resourceLang = newLanguages.FirstOrDefault(l => l.Culture.CaseInsensitiveEquals(r.Culture));
                    if (resourceLang != null)
                    {
                        _stringResourceLanguageRepository.Insert(r);
                    }
                    else
                    {
                        resourceLang = this.AddNotExistingLanguageResource(r);
                        newLanguages.Add(resourceLang);
                    }
                    changedCount++;
                }
            }
                
            await _languageRepository.CommitChangesAsync();

            foreach (var c in existingLanguages.Select(l=>l.Culture))
            {
                _cacheManager.Remove(GetLanguageCacheKey(c));
            }
            return changedCount;
        }

        public async Task<bool> AddStringResourceAsync(StringResource resource, AdditionPolicy policy)
        {
            Guard.ArgumentNotNull(resource, nameof(resource));
            ThrowIfInvalidResource(resource);
            bool changed = false;
            Language lang = await GetLanguageWithoutCacheAsync(resource.Culture, true);
            if (lang == null)
            {
                lang = AddNotExistingLanguageResource(resource);
                changed = true;
            }
            else
            {
                changed = AddExsitingLanguageResource(resource, policy, lang);
            }

            await _stringResourceLanguageRepository.CommitChangesAsync();

            _cacheManager.Remove(GetLanguageCacheKey(resource.Culture));
            return changed;
        }

        private Language AddNotExistingLanguageResource(StringResource resource)
        {
            Language lang = SchubertUtility.CreateLanguage(resource.Culture);
            _languageRepository.Insert(lang);
            _stringResourceLanguageRepository.Insert(resource);
            return lang;
        }

        private bool AddExsitingLanguageResource(StringResource resource, AdditionPolicy policy, Language lang)
        {
            bool changed;
            var existing = lang.StringResources.FirstOrDefault(r => r.ResourceName.CaseInsensitiveEquals(resource.ResourceName));
            if (existing != null)
            {
                switch (policy)
                {
                    case AdditionPolicy.ReplaceExisting:
                        existing.ResourceName = resource.ResourceName;
                        existing.ResourceValue = resource.ResourceValue;
                        existing.IsStatic = resource.IsStatic;
                        _stringResourceLanguageRepository.Update(existing);
                        changed = true;
                        break;
                    case AdditionPolicy.SkipExisting:
                    default:
                        changed = false;
                        break;
                }
            }
            else
            {
                _stringResourceLanguageRepository.Insert(resource);
                changed = true;
            }

            return changed;
        }

        private static void ThrowIfInvalidResource(StringResource resource)
        {
            if (resource.Culture.IsNullOrWhiteSpace())
            {
                throw new ArgumentException($"{nameof(resource)}.{nameof(StringResource.Culture)} can not be null.", nameof(resource));
            }
            if (resource.ResourceName.IsNullOrWhiteSpace())
            {
                throw new ArgumentException($"{nameof(resource)}.{nameof(StringResource.ResourceName)} can not be null.", nameof(resource));
            }
            if (resource.ResourceName.IsNullOrWhiteSpace())
            {
                throw new ArgumentException($"{nameof(resource)}.{nameof(StringResource.ResourceValue)} can not be null.", nameof(resource));
            }
        }

        public async Task CreateLanguageAsync(Language language)
        {
            Guard.ArgumentNotNull(language, nameof(language));

            _languageRepository.Insert(language);
            await _languageRepository.CommitChangesAsync();
        }

        public async Task DeleteLanguageAsync(string culture)
        {
            if (culture.IsNullOrWhiteSpace())
            {
                return;
            }
            Language lang = await (from l in _languageRepository.Table
                                   where l.Culture == culture
                                   select l)
                                   .Include(l => l.StringResources)
                                  .FirstOrDefaultAsync();
            if (lang != null)
            {
                foreach (var r in lang.StringResources)
                {
                    _stringResourceLanguageRepository.Delete(r);
                }

                _languageRepository.Delete(lang);
                await _languageRepository.CommitChangesAsync();
                _cacheManager.Remove(GetLanguageCacheKey(culture));
            }
        }

        public async Task<Language> GetLanguageAsync(string culture)
        {
            if (culture.IsNullOrWhiteSpace())
            {
                return null;
            }
            Guard.ArgumentNullOrWhiteSpaceString(culture, nameof(culture));
            return await _cacheManager.GetOrSetAsync(GetLanguageCacheKey(culture), k =>
            {
                return GetLanguageWithoutCacheAsync(culture);
            }, TimeSpan.FromMinutes(10), useSlidingExpiration: true);
        }

        private async Task<Language> GetLanguageWithoutCacheAsync(string culture, bool tracking = false)
        {
            if (culture.IsNullOrWhiteSpace())
            {
                return null;
            }
            var table = tracking ? _languageRepository.Table : _languageRepository.TableNoTracking;
            
            var query = (from l in table
                         where l.Culture == culture
                         select l).Include(l => l.StringResources);
            var lang = await query.FirstOrDefaultAsync();
            return lang;
        }

        public async Task<IEnumerable<Language>> GetPublishedLanguagesAsync()
        {
            var query = from l in _languageRepository.Table
                        where l.Published == true
                        select l;
            return await query.ToArrayAsync();
        }

        public async Task PublishLanguageAsync(string culture)
        {
            if (culture.IsNullOrWhiteSpace())
            {
                return;
            }
            await SetLanguagePublish(culture, true);
        }

        private async Task SetLanguagePublish(string culture, bool published)
        {
            if (culture.IsNullOrWhiteSpace())
            {
                return;
            }
            var language = await GetLanguageWithoutCacheAsync(culture, true);
            if (language != null && language.Published != published)
            {
                language.Published = true;
                _languageRepository.Update(language);
                await _languageRepository.CommitChangesAsync();

                _cacheManager.Remove(GetLanguageCacheKey(culture));
            }
        }

        public async Task RemoveStringResourceAsync(string culture, string resourceName)
        {
            if (culture.IsNullOrWhiteSpace() || resourceName.IsNullOrWhiteSpace())
            {
                return;
            }
            var query = from r in _stringResourceLanguageRepository.Table
                        where r.Culture == culture && r.ResourceName == resourceName
                        select r;
            var resource = await query.FirstOrDefaultAsync();
            if (resource != null)
            {
                _stringResourceLanguageRepository.Delete(resource);
                await _stringResourceLanguageRepository.CommitChangesAsync();
                _cacheManager.Remove(GetLanguageCacheKey(culture));
            }
        }

        public async Task UnpublishAsync(string culture)
        {
            if (culture.IsNullOrWhiteSpace())
            {
                return;
            }
            await this.SetLanguagePublish(culture, false);
        }

        public async Task UpdateLanguageAsync(Language language)
        {
            Guard.ArgumentNotNull(language, nameof(language));
            _languageRepository.Update(language);
            await _languageRepository.CommitChangesAsync();

            _cacheManager.Remove(GetLanguageCacheKey(language.Culture));
        }
    }
}
