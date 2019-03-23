using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    /// <summary>
    /// 表示 Json 拼写风格的枚举。
    /// </summary>
    public enum JsonCaseStyle
    {
        /// <summary>
        /// 表示小驼峰（JAVA ）风格。
        /// </summary>
        CamelCase,
        /// <summary>
        /// 表示大驼峰（C#）风格。
        /// </summary>
        PascalCase,
    }
}
