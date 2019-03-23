using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Schubert.Framework.Configuration;

namespace Schubert.Framework
{
    public static class DbConfigurationSourceExtensions
    {
        /// <summary>
        /// 添加一个数据库存储的配置。
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="tempJsonFile">
        /// Json 模版文件（当存在该参数指定的 json 文件时会自动将 json 中的配置更新到数据库）。
        /// 通常此参数在过渡环境使用，生产环境不应该使用此参数而应该直接读取数据库。
        /// </param>
        /// <param name="region">配置存储的区域。</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddDbConfiguration(
            this IConfigurationBuilder configuration,
            string tempJsonFile = null, string region = "global")
        {
            configuration.Add(new DbConfigurationSource(tempJsonFile, region));

            return configuration;
        }

    }
}
