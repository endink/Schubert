using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 用于标识数据组件的上下文范围（和 <see cref="DbContextAttribute"/> 类似，但可以通过类型名称构造，不需要强引用）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited =false, AllowMultiple = false)]
    public sealed class DbContextSelectionAttribute : Attribute
    {
        public Type ContextType { get; }

        /// <summary>
        /// 创建 <see cref="DbContextSelectionAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="contextType">数据上下文类型（<see cref="DbContext"/> 类型）。</param>
        public DbContextSelectionAttribute(Type contextType)
        {
            Guard.ArgumentNotNull(contextType, nameof(contextType));
            Guard.TypeIsAssignableFromType(contextType, typeof(DbContext), nameof(contextType));
            this.ContextType = contextType;
        }

        public DbContextSelectionAttribute(string contextType)
            :this(Type.GetType(contextType))
        {
        }
    }
}
