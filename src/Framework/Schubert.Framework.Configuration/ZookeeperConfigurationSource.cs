using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Configuration
{
    /// <summary>
    /// 表示一个基于 Zookeeper 的数据源。
    /// </summary>
    public class ZookeeperConfigurationSource : IConfigurationSource
    {
        private ConfigurationCenterOptions _options = null;
        private String _customConfigFile = null;
        public ZookeeperConfigurationSource(ConfigurationCenterOptions options, String customConfigFile = null)
        {
            _options = options;
            _customConfigFile = customConfigFile;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ZookeeperConfigurationProvider(_options, _customConfigFile);
        }
    }
}
