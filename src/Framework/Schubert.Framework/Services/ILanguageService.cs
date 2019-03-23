using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Services
{
    /// <summary>
    /// 为应用程序提供全球化实现的语言服务。
    /// </summary>
    public interface ILanguageService : IDependency
    {
        /// <summary>
        /// 创建一种系统语言。
        /// </summary>
        /// <param name="language"><see cref="Language"/> 对象。</param>
        Task CreateLanguageAsync(Language language);

        /// <summary>
        /// 更新特定区域的系统语言。
        /// </summary>
        /// <param name="language"><see cref="Language"/> 对象。</param>
        Task UpdateLanguageAsync(Language language);

        /// <summary>
        /// 删除特定区域名称（遵循 RCF 4646 标准）的语言。
        /// </summary>
        /// <param name="culture">语言的特定区域名称（遵循 RCF 4646 标准）。</param>
        Task DeleteLanguageAsync(string culture);

        /// <summary>
        /// 根据给定的特定区域名称（遵循 RCF 4646 标准）获取语言对象。
        /// </summary>
        /// <param name="culture">语言的特定区域名称（遵循 RCF 4646 标准）</param>
        /// <returns><see cref="Language"/> 对象。</returns>
        Task<Language> GetLanguageAsync(string culture);

        /// <summary>
        /// 获取系统中所有已经发布的语言。
        /// </summary>
        /// <returns>包含发布语言特定区域名称的集合。</returns>
        Task<IEnumerable<Language>> GetPublishedLanguagesAsync();

        /// <summary>
        /// 发布系统中已有的特定区域语言，如果没有找到语言，不应该抛出异样。
        /// </summary>
        /// <param name="culture">语言的特定区域名称（遵循 RCF 4646 标准）。</param>
        Task PublishLanguageAsync(string culture);

        /// <summary>
        /// 对已经发布的语言取消发布，语言的特定区域名称。
        /// </summary>
        /// <param name="culture">语言的特定区域名称（遵循 RCF 4646 标准）。</param>
        Task UnpublishAsync(string culture);

        /// <summary>
        /// 添加特定区域语言的字符串资源。
        /// </summary>
        /// <param name="resource">要添加的资源对象。</param>
        /// <param name="policy">添加策略。</param>
        /// <returns>表示是否添加成功，为 true 表示成功，false 表示被跳过。</returns>
        Task<bool> AddStringResourceAsync(StringResource resource, AdditionPolicy policy = AdditionPolicy.SkipExisting);

        /// <summary>
        /// 批量添加特定区域语言的字符串资源。
        /// </summary>
        /// <param name="resources">要添加的资源集合。</param>
        /// <param name="policy">添加策略。</param>
        /// <returns>表示添加成功的记录数（对于跳过的项不会计数）。</returns>
        Task<int> AddStringResourcesAsync(IEnumerable<StringResource> resources, AdditionPolicy policy = AdditionPolicy.ReplaceExisting);

        /// <summary>
        /// 移除特定区域语言中的字符串资源。
        /// </summary>
        /// <param name="culture">语言的特定区域名称（遵循 RCF 4646 标准）。</param>
        /// <param name="resourceName">要移除的资源名称。</param>
        Task RemoveStringResourceAsync(string culture, string resourceName);
    }

    public static class ILanguageServiceExtensions
    {
        /// <summary>
        /// 创建特定区域的语言。
        /// </summary>
        /// <param name="service">语言服务。</param>
        /// <param name="culture">特定区域名称。</param>
        /// <returns>创建的 <see cref="Language"/> 对象。</returns>
        public static async Task<Language> CreateLanguageAsync(this ILanguageService service, string culture)
        {
            Language language = SchubertUtility.CreateLanguage(culture);
            language.Culture = culture;
            await service.CreateLanguageAsync(language);
            return language;
        }
    }
}
