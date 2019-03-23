using System;
using Schubert.Framework.Environment.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Events;

namespace Schubert.Framework.Environment.ShellBuilders
{
    /// <summary>
    /// 应用程序 Shell 上下文。
    /// </summary>
    public sealed class ShellContext
    {
        /// <summary>
        /// 获取或设置应用程序蓝图。
        /// </summary>
        public ShellBlueprint Blueprint { get; set; }

        /// <summary>
        /// 获取或设置应用程序组件信息。
        /// </summary>
        public ShellDescriptor Descriptor { get; set; }

        /// <summary>
        /// Shell 上下文中的服务（这里服务指 DI 中的依赖项）描述。
        /// </summary>
        public IEnumerable<SmartServiceDescriptor> Services { get; set; }

        /// <summary>
        /// 获取 Shell 上下文中已经注册的服务。
        /// </summary>
        public IServiceCollection RegisteredServices { get; internal set; } = new ServiceCollection();

        /// <summary>
        /// Shell 上下文中的事件订阅描述。
        /// </summary>
        public IEnumerable<EventSubscriptionDescriptor> EventSubscriptors { get; set; }
    }
}