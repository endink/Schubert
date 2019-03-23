using FluentValidation;
using Schubert.Framework;

namespace Schubert.Framework.Web.Validation
{
    /// <summary>
    /// 表示一个自动注入的验证器。
    /// </summary>
    public interface IDependencyValidator : ITransientDependency
    { }

    /// <summary>
    /// 从此类派生的类将自动加载到上下文中作为数据验证器。
    /// </summary>
    /// <typeparam name="T">要验证的模型对象。</typeparam>
    public abstract class DependencyValidator<T> : AbstractValidator<T>, IDependencyValidator
    {
    }
}
