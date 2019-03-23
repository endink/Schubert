
using System;

namespace Schubert.Framework
{
    /// <summary>
    /// 标记一个类型为依赖项（使用此接口的依赖项的生命周期为默认为 per request ）。
    /// </summary>
    public interface IDependency
    {
    }

    /// <summary>
    /// 标记一个类型为依赖项（使用此接口的依赖项生命周期为 singleton）。
    /// </summary>
    public interface ISingletonDependency : IDependency
    {
    }

    /// <summary>
    /// 标记一个类型为依赖项（使用此接口的依赖项生命周期为 per usage）。 
    /// </summary>
    public interface ITransientDependency : IDependency
    {
    }

    //public abstract class Component : IDependency
    //{
    //    protected Component()
    //    {
    //        Logger = NullLogger.Instance;
    //        T = NullLocalizer.Instance;
    //    }

    //    public ILogger Logger { get; set; }
    //    public Localizer T { get; set; }
    //}
}