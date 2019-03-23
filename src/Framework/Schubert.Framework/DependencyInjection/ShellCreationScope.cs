using Microsoft.Extensions.Logging;
using Schubert.Framework.FileSystem.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.Environment;

namespace Schubert.Framework.DependencyInjection
{
    /// <summary>
    /// 表示创建 Shell 的环境配置。
    /// </summary>
    public sealed  class ShellCreationScope
    {

        internal Action<ILoggingBuilder> LoggingConfigure { get; set; }

        public void ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            this.LoggingConfigure += configure;
        }
    }
}
