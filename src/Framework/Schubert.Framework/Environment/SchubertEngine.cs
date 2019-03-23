using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Schubert.Framework.Environment
{
    /// <summary>
    /// Schubert Framework 引擎对象，提供基础 Schubert 运行时功能， 请从 <see cref="SchubertEngine.Current"/> 属性访问实例。
    /// </summary>
    public class SchubertEngine : IServiceProvider
    {
        private static SchubertEngine _current;
        private static readonly object SyncObject = new object();
        private bool _isRunning = false;
        private IServiceProvider _serviceProvider;
        private ISchubertEnvironment _environment;

        private SchubertEngine() { }

        /// <summary>
        /// 获取应用程序的名称（系统名称，通过配置文件获取）。
        /// </summary>
        public string ApplicationName { get; internal set; }

        /// <summary>
        /// 获取应用程序版本。
        /// </summary>
        public string ApplicationVersion { get; internal set; }

        /// <summary>
        /// 初始化 <see cref="SchubertEngine"/> 实例，开发人员不应调用该方法，仅限 Schubert Framework 内部使用。
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> 对象。</param>
        public void Start(IServiceProvider serviceProvider)
        {
            if (!_isRunning)
            {
                Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
                //获取基本信息。
                _serviceProvider = serviceProvider;
                _isRunning = true;
            }
        }

        /// <summary>
        /// 指示引擎实例是否已经启动，未启动大多数方法调用将抛出异常。
        /// </summary>
        public bool IsRunning { get { return _isRunning; } }

        /// <summary>
        /// 获取一直值指示应用程序是否处于开发环境。
        /// </summary>
        public bool? IsDevelopmentEnvironment
        {
            get { return _environment?.IsDevelopmentEnvironment; }
        }

        /// <summary>
        /// 获取应用程序当前的运行时 Framework 名称（包含平台标识，版本号等信息。 关于 <see cref="System.Runtime.Versioning.FrameworkName"/> 类更多请参考 MSDN）。
        /// </summary>
        public String FrameworkName { get; internal set; }

        /// <summary>
        /// 获取一值，只是当前环境中 Shell 是否已经创建。
        /// </summary>
        public bool ShellCreated { get; internal set; }

        ///// <summary>
        ///// 获取应用程序当前的运行时 Framework （暂时只支持判断 Microsoft 平台，Mono等平台可能显示为 Unknown）。
        ///// </summary>
        //public RuntimeFramework Framework
        //{
        //    get
        //    {
        //        if (this.FrameworkName == null)
        //        {
        //            return RuntimeFramework.Unknown;
        //        }
        //        switch (this.FrameworkName.Identifier)
        //        {
        //            case "DNXCore":
        //                return RuntimeFramework.DNXCore;
        //            case "DNX":
        //                return RuntimeFramework.DNX;
        //            case ".NetFramework":
        //                return RuntimeFramework.Net;
        //            default:
        //                return RuntimeFramework.Unknown;
        //        }
        //    }
        //}

        internal void LoadEnvironment(IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            ISchubertEnvironment hosting = serviceProvider.GetService<ISchubertEnvironment>();
            _environment = hosting;
            this.FrameworkName = hosting.RuntimeFramework;
        }

#region RunTime Objects
        

        public WorkContext GetWorkContext()
        {
            ThrowIfNotStarted();
            var accessor = _serviceProvider.GetService<IWorkContextAccessor>();
            if (accessor == null)
            {
                throw new SchubertException($"必须为当前环境实现单例模式的 {nameof(IWorkContextAccessor)} 接口才能获取工作上下文。");
            }
            return accessor.GetContext();
        }

        /// <summary>
        /// 创建一个 DI 容器的 Scope。
        /// </summary>
        /// <returns></returns>
        public IServiceScope CreateScope()
        {
            ThrowIfNotStarted();
            var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
            return serviceScopeFactory.CreateScope();
        }

        /// <summary>
        /// 创建一个类型实例（自动进行构造函数依赖注入）。
        /// </summary>
        /// <param name="arguments">DI 容器中未提供构造函数参数。</param>
        /// <returns>通过 DI 构造的实例。</returns>
        public T CreateInstance<T>(params object[] arguments)
        {
            ThrowIfNotStarted();
            return ActivatorUtilities.CreateInstance<T>(_serviceProvider, arguments);
        }

        /// <summary>
        /// 创建一个类型实例（自动进行构造函数依赖注入）。
        /// </summary>
        /// <param name="instanceType">要创建的实力类型。</param>
        /// <param name="arguments">DI 容器中未提供构造函数参数。</param>
        /// <returns>通过 DI 构造的实例。</returns>
        public object CreateInstance(Type instanceType, params object[] arguments)
        {
            ThrowIfNotStarted();
            Guard.ArgumentNotNull(instanceType, nameof(instanceType));
            return ActivatorUtilities.CreateInstance(_serviceProvider, instanceType, arguments);
        }

        private void ThrowIfNotStarted()
        {
            if (!_isRunning)
            {
                throw new SchubertException("The schubert engine was not start.");
            }
        }

        public object GetRequiredService(Type serviceType)
        {
            ThrowIfNotStarted();
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));
            return _serviceProvider == null ? null : _serviceProvider.GetRequiredService(serviceType);
        }

        public object GetService(Type serviceType)
        {
            ThrowIfNotStarted();
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));
            return _serviceProvider == null ? null : _serviceProvider.GetService(serviceType);
        }

        public TService GetService<TService>()
        {
            ThrowIfNotStarted();
            return _serviceProvider == null ? default(TService) : _serviceProvider.GetService<TService>();
        }

        public TService GetRequiredService<TService>()
        {
            ThrowIfNotStarted();
            return _serviceProvider == null ? default(TService) : _serviceProvider.GetRequiredService<TService>();
        }

        public IEnumerable<T> GetServices<T>()
        {
            ThrowIfNotStarted();
            return _serviceProvider == null ? Enumerable.Empty<T>() : _serviceProvider.GetServices<T>();
        }

        #endregion


        /// <summary>
        /// 获取当前 <see cref="SchubertEngine"/> 实例。
        /// </summary>
        public static SchubertEngine Current
        {
            get
            {
                if (_current == null)
                {
                    lock(SyncObject)
                    {
                        if (_current == null)
                        {
                            _current = new SchubertEngine();
                        }
                    }
                }
                return _current;
            }
        }

        public string GroupName { get; internal set; }

        public String Environment
        {
            get { return _environment?.Environment; }
        }
    }
}