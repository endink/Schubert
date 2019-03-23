using System;

namespace Schubert.Framework.Localization
{
    /// <summary>
    /// 获取指定资源键的本地化字符串（如果键不存在获取 defaultString）。
    /// </summary>
    /// <param name="key">要获取的本地化资源的键。</param>
    /// <param name="defaultString">在资源未找到时返回的默认字符串。</param>
    /// <param name="args">用于格式化本地化资源的参数。</param>
    /// <returns></returns>
    public delegate String Localizer(string key, string defaultString, params object[] args);
}