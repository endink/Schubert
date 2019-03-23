using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.Domain;
using System.Globalization;

namespace Schubert.Framework.Services
{
    public class NullLanguageService : ILanguageService
    {
        public Task AddStringResourceAsync(StringResource resource)
        {
            return Task.FromResult(0);
        }

        public Task AddStringResourcesAsync(IEnumerable<StringResource> resources)
        {
            return Task.FromResult(0);
        }

        public Task CreateLanguageAsync(Language language)
        {
            return Task.FromResult(0);
        }

        public Task DeleteLanguageAsync(string culture)
        {
            return Task.FromResult(0);
        }

        public Task<Language> GetLanguageAsync(string culture)
        {
            return Task.FromResult<Language>(null);
        }

        public Task PublishLanguageAsync(string culture)
        {
            return Task.FromResult(0);
        }

        public Task RemoveStringResourceAsync(string culture, string resourceName)
        {
            return Task.FromResult(0);
        }

        public Task UnpublishAsync(string culture)
        {
            return Task.FromResult(0);
        }

        public Task UpdateLanguageAsync(Language language)
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<Language>> GetPublishedLanguagesAsync()
        {
            return Task.FromResult(Enumerable.Empty<Language>());
        }

        public Task<bool> AddStringResourceAsync(StringResource resource, AdditionPolicy policy = AdditionPolicy.SkipExisting)
        {
            return Task.FromResult(false);
        }

        public Task<int> AddStringResourcesAsync(IEnumerable<StringResource> resources, AdditionPolicy policy = AdditionPolicy.ReplaceExisting)
        {
            return Task.FromResult(0);
        }
    }
}
