using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Schubert.Framework
{
    /// <summary>
    /// 实现此接口声明应用程序依赖项。推荐使用 <see cref="IDependency"/> 接口自动注册，而不是实现此接口手工注册。
    /// 此接口注册服务时优先级高于 <see cref="IDependency"/>。
    /// </summary>
    public interface IDependencyDescriber
    {
        /// <summary>
        /// 创建要注入到容器中的服务。
        /// </summary>
        /// <returns>注册的服务集合。</returns>
        IEnumerable<SmartServiceDescriptor> Build();
    }
}