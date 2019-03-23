using System;

namespace Schubert.Framework.Domain
{
    /// <summary>
    /// 表示一个特定语言文化中的字符串资源。
    /// </summary>
    public class StringResource
    {
        public StringResource()
        {
            this.IsStatic = true;
        }

        /// <summary>
        /// 获取或设置字符串资源的特定区域名称（遵循 RCF 4646 标准）。
        /// </summary>
        public string Culture { get; set; }
        /// <summary>
        /// 获取火设置资源名称。
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// 获取或设置资源的值。
        /// </summary>
        public string ResourceValue { get; set; }

        /// <summary>
        /// 获取或设置一个值，指示资源是否为静态资源（静态资源是存在于UI或系统中的固定文本，动态资源通常是为动态数据全球化提供支持的文本。）。
        /// </summary>
        public bool IsStatic { get; set; }
        
    }
}