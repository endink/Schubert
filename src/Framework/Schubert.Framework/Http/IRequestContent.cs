using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    /// <summary>
    /// 表示一个请求内容。
    /// </summary>
    public interface IRequestContent
    {
        /// <summary>
        /// 获取 HTTP 请求内容。
        /// </summary>
        /// <returns></returns>
        HttpContent GetContent();
    }
}
