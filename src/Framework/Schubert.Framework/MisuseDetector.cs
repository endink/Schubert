using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    /// <summary>
    /// 用于防止资源滥用的类。
    /// </summary>
    public class MisuseDetector
    {
        private Type _type;
        private long _activeInstances;
        private int _logged = 0;
        private readonly int _maxInstance;
        private ILogger _logger;

        public MisuseDetector(Type type, ILogger logger, int maxInstances)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            if (maxInstances < 1)
            {
                throw new ArgumentException();
            }
            this._type = type;
            this._maxInstance = maxInstances;
            this._logger = logger ?? (ILogger)NullLogger.Instance;
        }

        public MisuseDetector(Type type, ILoggerFactory loggerFactory, int maxInstances)
            :this(type, loggerFactory?.CreateLogger<Type>(), maxInstances)
        {
        }

        /// <summary>
        /// 增加资源是的使用计数。
        /// </summary>
        public void Increase()
        {
            Task.Run(() =>
            {
                if (Interlocked.Increment(ref _activeInstances) > _maxInstance)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        if (Interlocked.CompareExchange(ref _logged, 0, 1) == 0)
                        {
                            _logger.LogWarning(
                                    $"检测到创建过多的 {_type.Name} 类型实例（实例警告上限：{_maxInstance}）, {_type.Name} 是稀缺资源，请考虑减少它的实例（当前是实例：{_activeInstances}）。");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 减少资源的使用计数。
        /// </summary>
        public void Decrease()
        {
            Task.Run(() => Interlocked.Decrement(ref _activeInstances));
        }
    }
       
}
