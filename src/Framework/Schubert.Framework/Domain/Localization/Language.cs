using System.Collections.Generic;

namespace Schubert.Framework.Domain
{
    /// <summary>
    /// 表示一个语言。
    /// </summary>
    public class Language
    {
        private ICollection<StringResource> _localeStringResources;

        public Language()
        {
            this.Published = true;
        }

        /// <summary>
        /// 获取设置语言的名称（遵循微软规则，对于 .Net 程序推荐和 CultureInfo.DisplayName  保持一致规则，参考 :https://msdn.microsoft.com/zh-cn/library/system.globalization.cultureinfo.displayname(v=vs.110).aspx）。
        /// </summary>
        public string DisplayName { get; set; } = "简体中文";

        /// <summary>
        /// 语言文化的区域名称（应该遵循  RFC 4646 标准）。可以参考：https://msdn.microsoft.com/zh-cn/library/system.globalization.cultureinfo(v=vs.110).aspx
        /// </summary>
        public string Culture { get; set; } = "zh-Hans";

        /// <summary>
        /// SEO代码（一般来说遵循 ISO 3166 标准，参考：http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2）。
        /// </summary>
        public string UniqueSeoCode { get; set; }

        /// <summary>
        /// 语言的标志图案（通常为一个国旗图标）。
        /// </summary>
        public string FlagImageFileName { get; set; }

        /// <summary>
        /// 表示语言是否已发布（未发布的语言将不可用）。
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// 在语言选择器中的排序位置。
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 获取或设置语言下的字符串文本。
        /// </summary>
        public ICollection<StringResource> StringResources
        {
            get { return _localeStringResources ?? (_localeStringResources = new List<StringResource>()); }
            protected set { _localeStringResources = value; }
        }
    }
}