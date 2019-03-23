using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Reflection;

namespace Schubert
{
    /// <summary>
    /// 通过动态类型包装一个反射对象。
    /// </summary>
    public class ReflectionObject : DynamicObject
    {
        private object m_object;
        private BindingFlags m_flags;
        private Type m_reflectionType;

        public ReflectionObject(object instance, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            : this(instance, (instance == null ? null : instance.GetType()), flags)
        {

        }

        public ReflectionObject(object instance, Type reflectionType, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            Guard.ArgumentNotNull(instance, "instance");
            Guard.ArgumentNotNull(reflectionType, "reflectionType");
            this.m_object = instance;
            this.m_flags = flags;
            this.m_reflectionType = reflectionType;
        }

        public override bool TryInvokeMember(
                InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            // Find the called method using reflection
            var methodInfo = this.m_reflectionType.GetTypeInfo().GetMethod(
                binder.Name,
                this.m_flags);

            if (methodInfo == null)
            {
                return false;
            }

            // Call the method
            result = methodInfo.Invoke(m_object, args);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            // Find the called method using reflection
            var propertyInfo = this.m_reflectionType.GetTypeInfo().GetProperty(
                binder.Name,
                this.m_flags);

            if (propertyInfo == null || !propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length > 0)
            {
                return false;
            }

            // Call the method
            result = propertyInfo.GetValue(m_object, null);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // Find the called method using reflection
            var propertyInfo = this.m_reflectionType.GetTypeInfo().GetProperty(
                binder.Name,
                this.m_flags);

            if (propertyInfo == null || !propertyInfo.CanWrite || propertyInfo.GetIndexParameters().Length > 0)
            {
                return false;
            }

            // Call the method
            propertyInfo.SetValue(m_object, value, null);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = null;
            // Find the called method using reflection
            var propertyInfo = this.m_reflectionType.GetTypeInfo().GetProperty(
                binder.CallInfo.ArgumentNames[0],
                this.m_flags);

            if (propertyInfo == null || !propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length != indexes.Length)
            {
                return false;
            }

            // Call the method
            var value = propertyInfo.GetValue(m_object, indexes);

            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            // Find the called method using reflection
            var propertyInfo = this.m_reflectionType.GetTypeInfo().GetProperty(
                binder.CallInfo.ArgumentNames[0],
                this.m_flags);

            if (propertyInfo == null || !propertyInfo.CanWrite || propertyInfo.GetIndexParameters().Length != indexes.Length)
            {
                return false;
            }

            // Call the method
            propertyInfo.SetValue(m_object, value, indexes);
            return true;
        }

    }
}
