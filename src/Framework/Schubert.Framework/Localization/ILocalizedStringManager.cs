using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Localization
{
    /// <summary>
    /// 提供本地化字符串管理的类。
    /// </summary>
    public interface ILocalizedStringManager
    {
        /// <summary>
        /// 获取特定区域的字符串资源。
        /// </summary>
        /// <param name="cultureName">特定区域名称。</param>
        /// <param name="key">字符串资源键。</param>
        /// <returns>字符串资源，如果找不到，返回 null。</returns>
        string GetLocalizedString(string cultureName, string key);

        /// <summary>
        /// 获取制定语言文化的所有资源键。
        /// </summary>
        /// <param name="culture">要从中获取字符串资源的语言文件。</param>
        /// <returns>资源键的集合。</returns>
        Task<IEnumerable<String>> GetLocalizedStringKeys(string culture);

        /// <summary>
        /// 导出特定区域的字符串资源为 XML 文本。
        /// </summary>
        /// <param name="cultureName">特定区域名称。</param>
        /// <returns>XML 文本。</returns>
        Task<String> ExportLanguageXmlAsync(string cultureName);
        /// <summary>
        /// 导入 XML 中包含的特定区域字符串资源。
        /// </summary>
        /// <param name="cultureName">特定区域名称。</param>
        /// <param name="xml">XML 文本。</param>
        /// <param name="policy">导入策略，指示当导入的 XML 中包含已经存在的资源时如果处理。</param>
        Task ImportLanguageXmlAsync(string cultureName, string xml, AdditionPolicy policy = AdditionPolicy.ReplaceExisting);
    }
}
