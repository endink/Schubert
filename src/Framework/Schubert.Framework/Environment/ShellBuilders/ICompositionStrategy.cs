using Schubert.Framework.Environment.Modules;
using System;

namespace Schubert.Framework.Environment.ShellBuilders
{
    /// <summary>
    /// 实现此接口，以提供应用程序组建的组织策略。
    /// </summary>
    public interface ICompositionStrategy
    {
        /// <summary>
        /// 从 <see cref="Schubert.Framework.Environment.Modules.IFeatureManager"/> 获取应用程序组件信息，
        /// 并构建应用程序蓝图（<see cref="ShellBlueprint"/> 对象），蓝图将被 IOC 容器用来构建应用程序（业务开发人员无需关心此接口）。
        /// </summary>
        ShellBlueprint Compose(string applicationName, ShellDescriptor descriptor);
    }
}