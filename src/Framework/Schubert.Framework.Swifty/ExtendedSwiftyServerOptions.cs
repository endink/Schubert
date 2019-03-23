using Swifty.MicroServices.Server;
using System; 

namespace Schubert.Framework.Swifty
{
    public class ExtendedSwiftyServerOptions : SwiftyServerOptions
    {
        public ExtendedSwiftyServerOptions()
        {
            this.Port = 5991;
        }

        /// <summary>
        /// 获取或设置当使用配置中心注册服时当前实例公开的地址（ip 地址、主机名）。
        /// 如果为 null 或空串将使用 <see cref="NetworkOptions"/> 配置中的地址匹配。
        /// </summary>
        public String PublicAddress { get; set; }
    }
}
