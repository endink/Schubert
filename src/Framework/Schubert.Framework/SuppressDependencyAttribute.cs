using System;

namespace Schubert.Framework
{
    /// <summary>
    /// 在接口实现上应用此特性可以替换接口的原有实现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class SuppressDependencyAttribute : Attribute
    {
        /// <summary>
        /// 创建 <see cref="SuppressDependencyAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="type">要替换的原有实现的类型。</param>
        public SuppressDependencyAttribute(Type type) : this(type.FullName)
        {

        }

        /// <summary>
        /// 创建 <see cref="SuppressDependencyAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="typeFullName">要替换的原有实现的类型名称（含命名空间不含程序集信息）。</param>
        public SuppressDependencyAttribute(string typeFullName)
        {
            TypeFullName = typeFullName;
        } 
        /// <summary>
        /// 获取被替换的类型的名称（包含命名空间但不含程序集信息的类型名称）。
        /// </summary>
        public string TypeFullName { get; }
    }
}