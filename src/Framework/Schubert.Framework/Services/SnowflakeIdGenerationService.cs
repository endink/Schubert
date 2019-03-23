using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Schubert.Framework.Environment;

namespace Schubert.Framework.Services
{
    /// <summary>
    /// 基于 twitter 算法的唯一 Id 生成器。
    /// </summary>
    public class SnowflakeIdGenerationService : IIdGenerationService
    {
        private SnowflakeWorker _worker = null;

        public SnowflakeIdGenerationService(
            IOptions<NetworkOptions> networkOptions, 
            ISchubertEnvironment schubertEnvironment, 
            ILoggerFactory loggerFactory)
        {
            Guard.ArgumentNotNull(networkOptions, nameof(networkOptions));
            Guard.ArgumentNotNull(schubertEnvironment, nameof(schubertEnvironment));

            var logger = loggerFactory?.CreateLogger<SnowflakeIdGenerationService>() ?? (ILogger)NullLogger.Instance;
            var lanOptions = networkOptions.Value.Lans.FirstOrDefault(x => x.DataCenterId == networkOptions.Value.DataCenterId);

            if ((networkOptions.Value.DataCenterId <= 0 || lanOptions == null) && !schubertEnvironment.IsDevelopmentEnvironment)
            {
                throw new InvalidOperationException($"未能从 Schubert:Network 中查找到合法的 DataCenterId，" +
                    $"{nameof(NetworkOptions)}.{nameof(NetworkOptions.DataCenterId)} 必须使用 {nameof(NetworkOptions)}.{nameof(NetworkOptions.Lans)} 中配置过的数据中心 id。");
            }

            var serverId = lanOptions?.GetServerId();
            var dataCenterId = networkOptions.Value.DataCenterId;
            if (!serverId.HasValue)
            {
                if (!schubertEnvironment.IsDevelopmentEnvironment)
                {
                    throw new SchubertException($"Snowflake 唯一 Id 生成配置错误，必须保证 {nameof(NetworkOptions)} 至少配置了一个{nameof(NetworkOptions.Lans)}，且与本机 Id 地址匹配。");
                }
                else
                {
                    logger.WriteWarning($"无法获取当应用所在的数据中心和 ip 地址，Snowflake id 生成算法将使用省缺配置（datacenter:1，worker: 1）。");
                    serverId = 1;
                    dataCenterId = 1;
                }
            }
            _worker = new SnowflakeWorker(serverId.Value - 1, dataCenterId - 1);
        }

        public long GenerateId()
        {
            return _worker.NextId();
        }
    }
}
