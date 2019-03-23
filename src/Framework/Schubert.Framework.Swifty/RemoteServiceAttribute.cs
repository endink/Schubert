using System;

namespace Schubert.Framework.Swifty
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Assembly)]
    public class RemoteServiceAttribute : Attribute
    {
        public string VipAddress { get; }
        public string Version { get; }
        public RemoteServiceAttribute(string vipAddress, string version)
        {
            Guard.ArgumentNullOrWhiteSpaceString(vipAddress, nameof(vipAddress));

            VipAddress = vipAddress;
            Version = version.IfNullOrWhiteSpace("1.0.0");
        }
        /// <summary>返回表示当前对象的字符串。</summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            return $"{nameof(this.VipAddress)}: {this.VipAddress}, {nameof(this.Version)}: {this.Version}";
        }
    }
}
