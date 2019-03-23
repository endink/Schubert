using Swifty;
using Swifty.MicroServices.Client;
using Swifty.Nifty.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Schubert.Framework.Swifty
{
    public class ExtendedSwiftyClientOptions : SwiftyClientOptions
    {
        private string[] _exploringAssemblies;
        private Dictionary<String, String> _directServices;

        private Dictionary<String, ClientSslConfig> _sslConfig;

        private WeakReference<ConcurrentDictionary<String, Regex>> _regexCache;

        /// <summary>
        /// 获取或设置客户端扫描的程序集，即远程服务的接口所在的程序集。
        /// </summary>
        public string[] ExploringAssemblies
        {
            get { return _exploringAssemblies ?? (_exploringAssemblies = new string[0]); }
            set { _exploringAssemblies = value; }
        }


        /// <summary>
        /// ssl config, allow use *
        /// </summary>
        public Dictionary<String, ClientSslConfig> Ssl
        {
            get { return _sslConfig ?? (_sslConfig = new Dictionary<String, ClientSslConfig>()); }
            set { _sslConfig = value; }
        }


        /// <summary>
        /// 获取或设置直连的（不通过注册中心客户端负载均衡连接，而是直接连接到服务端所在服务器）服务配置。
        /// 该配置的键为服务名称（与 <see cref="RemoteServiceAttribute"/> 一致）。
        /// 可以使用通配符 * 做批量配置。
        /// 该配置的键为服务地址（例如：192.168.1.2:3333）
        /// </summary>
        public Dictionary<String, String> DirectAddress
        {
            get { return _directServices ?? (_directServices = new Dictionary<String, String>()); }
            set { _directServices = value; }
        }

        /// <summary>
        /// 根据配置获取一个服务的 SSL 配置（与 <see cref="ClientSslConfig"/> 配置相关）。
        /// </summary>
        /// <param name="vipAddress">服务名称。</param>
        internal ClientSslConfig GetSslConfig(String vipAddress)
        {
            this.TryGetValue(this.Ssl, vipAddress, "ssl", out ClientSslConfig sslConfig);
            return sslConfig;
        }

        /// <summary>
        /// 根据配置获取一个服务的直连地址（与 <see cref="ExtendedSwiftyClientOptions.DirectAddress"/> 配置相关）。
        /// </summary>
        /// <param name="vipAddress">服务名称。</param>
        /// <param name="address">获取的直连地址。</param>
        /// <returns>一个布尔值，指示给定类型的服务是否配置直连地址，如果为 true，<paramref name="address"/> 将给出该地址，否则 <paramref name="address"/> 为 null。</returns>
        internal bool TryGetDirectAddress(String vipAddress, out String address)
        {
            return this.TryGetValue(this.DirectAddress, vipAddress, "direct", out address);
        }
        
        internal bool TryGetValue<TValue>(IDictionary<String, TValue> source, String vipAddress, string category, out TValue value)
        {
            value = default(TValue);
            if (vipAddress.IsNullOrWhiteSpace())
            {
                return false;
            }
            foreach (String key in source.Keys)
            {
                if (key.CaseSensitiveEquals(vipAddress)) //如果不是一个表达式, 快速比较提高性能。
                {
                    value = source[key];
                    return true;
                }
                Regex regex = GetServiceNameRegex(category, key);
                if (regex.IsMatch(vipAddress))
                {
                    value = source[key];
                    return true;
                }
            }

            return false;
        }

        private Regex GetServiceNameRegex(String category, string key)
        {
            ConcurrentDictionary<String, Regex> GetRegexCache()
            {
                ConcurrentDictionary<string, Regex> cache = null;
                if (_regexCache?.TryGetTarget(out cache) ?? false)
                {
                    return cache;
                }
                cache = new ConcurrentDictionary<string, Regex>();
                _regexCache = new WeakReference<ConcurrentDictionary<String, Regex>>(cache);
                return cache;
            }

            return GetRegexCache().GetOrAdd($"{category}:{key}", k =>
            {
                StringBuilder servicePattern = new StringBuilder("^");

                foreach (String c in key.Select(c => c.ToString()))
                {
                    String charString = c.Equals("*") ? "\\S" : Regex.Escape(c);
                    servicePattern.Append(charString);
                }
                Regex rg = new Regex(servicePattern.ToString(), RegexOptions.Compiled, TimeSpan.FromSeconds(10));
                return rg;
            });
        }
    }
}
