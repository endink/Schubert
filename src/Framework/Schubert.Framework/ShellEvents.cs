using Schubert.Framework.Environment.ShellBuilders;
using System;

namespace Schubert.Framework
{
    /// <summary>
    /// 用于订阅 Shell 创建完成事件的委托。
    /// </summary>
    /// <param name="options">当前 Schubert 配置选项。</param>
    /// <param name="context">Shell 上下文。</param>
    public delegate void ShellInitializedHandler(SchubertOptions options, ShellContext context);
    /// <summary>
    /// 用于订阅 引擎启动完成事件的委托。
    /// </summary>
    /// <param name="options">当前 Schubert 配置选项。</param>
    /// <param name="serviceProvider">应用程序级别的服务提供程序。</param>
    public delegate void EngineStartedHandler(SchubertOptions options, IServiceProvider serviceProvider);

    public delegate void DependencySetupHandler(SchubertOptions options, DependencySetupEventArgs eventArgs);

    /// <summary>
    /// 在 Schubert 框架中的静态事件。
    /// </summary>
    public static class ShellEvents
    {
        /// <summary>
        /// Shell 加载完成时发生。
        /// </summary>
        public static event ShellInitializedHandler ShellInitialized;
        /// <summary>
        /// 框架引擎启动时发生。
        /// </summary>
        public static event EngineStartedHandler EngineStarted;
        /// <summary>
        /// 向 DI 注册依赖项之前发生，可以替换依赖项（作为实现 Proxy 的扩展点）。
        /// </summary>
        public static event DependencySetupHandler DependencyRegistering;

        internal static void NotifyDependencyRegistering(SchubertOptions options, DependencySetupEventArgs args)
        {
            if (DependencyRegistering != null)
            {
                DependencyRegistering(options, args);
            }
        }

        internal static void NotifyShellInitialized(SchubertOptions options, ShellContext context)
        {
            if (ShellInitialized != null)
            {
                ShellInitialized(options, context);
                //清除委托列表，防止资源无法被 GC 收集。
                if (ShellInitialized != null) //再次判断，因为可能在事件订阅的方法中使用 “-=” 操作手动移除事件订阅使得委托链再次为空。
                {
                    foreach (var delegateItem in ShellInitialized.GetInvocationList())
                    {
                        ShellInitialized -= (ShellInitializedHandler)delegateItem;
                    }
                }
            }
        }

        internal static void NotifyEngineStarted(SchubertOptions options, IServiceProvider builder)
        {
            if (EngineStarted != null)
            {
                EngineStarted(options, builder);
                if (EngineStarted != null) //再次判断，因为可能在执行方法中使用 “-=” 操作手动移除事件订阅使得委托链再次为空。
                {
                    //清除委托列表，防止资源无法被 GC 收集。
                    foreach (var delegateItem in EngineStarted.GetInvocationList())
                    {
                        EngineStarted -= (EngineStartedHandler)delegateItem;
                    }
                }
            }
        }
    }
}
