/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-03-11 10:20:51 
===============================================================================
 Copyright ? Sharping.  All rights reserved.
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
 OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
 LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 FITNESS FOR A PARTICULAR PURPOSE.
===============================================================================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Schubert.Helpers;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace Schubert
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EnumDescriptionAttribute : Attribute
    {
        private string m_defaultDescription = null;

        public static string FlagSplitCharacter { get; set; } = " | ";

        public static bool CacheDescription { get; set; } = true;

        public EnumDescriptionAttribute()
        {
            this.m_defaultDescription = String.Empty;
        }

        public EnumDescriptionAttribute(string defaultDescription)
        {
            this.m_defaultDescription = defaultDescription;
        }

        public EnumDescriptionAttribute(string resourceName, Type resourceType)
        {
            Guard.ArgumentNotNull(resourceType, "resourceType");
            this.ResourceType = resourceType;
            this.ResourceName = resourceName;
        }

        private string GetResourceDescription(string resourceName, Type resourceType)
        {
            string description = String.Empty;
            PropertyInfo property = resourceType.GetTypeInfo().GetProperty(resourceName);
            bool flag = false;
            if (!resourceType.GetTypeInfo().IsVisible || property == null || property.PropertyType != typeof(string))
            {
                flag = true;
            }
            else
            {
                MethodInfo getMethod = property.GetGetMethod();

                if (getMethod == null || !getMethod.IsPublic || !getMethod.IsStatic)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                description =  (string)property.GetValue(null, null);
            }
            return description;
        }

        public string Description 
        {
            get { return this.m_defaultDescription; }
            set { this.m_defaultDescription = value; }
        }

        /// <summary>
        /// 从资源中获取枚举描述的资源类型。
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// 从资源文件中获取枚举描述的资源名称。
        /// </summary>
        public string ResourceName { get; set; }

        protected virtual string GetDescription()
        {
            if (this.ResourceType != null && this.ResourceName.IsNullOrWhiteSpace())
            {
                return this.GetResourceDescription(this.ResourceName, this.ResourceType);
            }
            else
            {
                return this.m_defaultDescription;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="description"></param>
        /// <param name="enumValue"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static bool TryParseEnumValue(Type enumType, string description, out object enumValue, bool ignoreCase = false)
        {
            enumValue = null;
            try
            {
                enumValue = ParseEnumValue(enumType, description, ignoreCase);
                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static IEnumerable<Object> GetEnumValues(Type enumType)
        {
            var values = Enum.GetValues(enumType).Cast<Object>();
            return values;
        }

        private static IEnumerable<String> GetEnumNames(Type enumType)
        {
            var values = Enum.GetNames(enumType);
            return values;
        }

        public static object ParseEnumValue(Type enumType, string description, bool ignoreCase = false)
        {
            if (!enumType.GetTypeInfo().IsEnum)
            {
                throw new ArgumentException(String.Format("{0} was not a enum type.", enumType.AssemblyQualifiedName));
            }
            try
            {
                object enumValue = Enum.Parse(enumType, description, ignoreCase);
                return enumValue;
            }
            catch (ArgumentException)
            {
                var values = GetEnumValues(enumType);
                object result = null;
                string[] descArray = description.Split(new string[] { EnumDescriptionAttribute.FlagSplitCharacter }, StringSplitOptions.RemoveEmptyEntries);

                var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

                foreach (object value in values)
                {
                    string desc = GetDescription((Enum)value);
                    if (!enumType.HasAttribute<FlagsAttribute>())
                    {
                        if (comparer.Equals(desc, description))
                        {
                            result = value;
                            break;
                        }
                    }
                    else
                    {
                        if (descArray.Contains(desc, comparer))
                        {
                            result = ((result == null) ? value : BinaryValues(enumType, ExpressionType.Or, result, value));
                        }
                    }
                }
                if (result != null)
                {
                    return result;
                }
            }
            throw new ArgumentException(String.Format("The description {0} is not define on enum {1}", description, enumType.Name));
        }

        private static readonly ConcurrentDictionary<Enum, string> DescriptionCache = new ConcurrentDictionary<Enum, string>();

        /// <summary>
        /// Get the enum description.
        /// </summary>
        /// <param name="enumValue">the enum value</param>
        /// <exception>TypeCompatibilityException</exception>
        public static string GetDescription(Enum enumValue)
        {
            Guard.ArgumentNotNull(enumValue, nameof(enumValue));
            if (!CacheDescription)
            {
                return GetDescriptionWithoutCache(enumValue.GetType(), enumValue);
            }
            else
            {
                return DescriptionCache.GetOrAdd(enumValue, v => GetDescriptionWithoutCache(v.GetType(), v));
            }
        }

        /// <summary>
        /// Get the enum description.
        /// </summary>
        /// <param name="enumType">the enum type</param>
        /// <param name="enumValue">the enum value</param>
        /// <exception>TypeCompatibilityException</exception>
        private static string GetDescriptionWithoutCache(Type enumType, object enumValue)
        {
            Guard.EnumValueIsDefined(enumType, enumValue, "enumValue");
            bool isFlagEnum = enumType.HasAttribute<FlagsAttribute>();
            Type underlyType = Enum.GetUnderlyingType(enumType);

            var names = Enum.GetNames(enumType);
            StringBuilder currentName = new StringBuilder();
           
            foreach (string name in names)
            {
                object value = Enum.Parse(enumType, name, false);
               
                if (!isFlagEnum) //普通枚举。
                {
                    if (value.Equals(enumValue))
                    {
                        currentName = new StringBuilder(name);
                        FieldInfo enumField = enumType.GetTypeInfo().GetField(name);
                        EnumDescriptionAttribute attribute = enumField.GetCustomAttribute<EnumDescriptionAttribute>();
                        if (attribute != null)
                        {
                            return attribute.GetDescription().IfNullOrWhiteSpace(enumValue.ToString());
                        }
                    }
                }
                else //位枚举。
                {
                    if (((Enum)enumValue).HasFlag((Enum)value))
                    {
                        if(Convert.ToInt64(value) == 0 && Convert.ToInt64(enumValue) != 0)
                        {
                            continue;
                        }
                        string text = name;
                        FieldInfo enumField = enumType.GetTypeInfo().GetField(name);
                        EnumDescriptionAttribute attribute = enumField.GetCustomAttribute<EnumDescriptionAttribute>();
                        if (attribute != null)
                        {
                            text = attribute.GetDescription().IfNullOrWhiteSpace(value.ToString());
                        }
                        currentName.Append(String.Format("{0}{1}", text, FlagSplitCharacter));
                    }
                }
            }

            if (isFlagEnum && currentName.Length > 0 && !FlagSplitCharacter.IsNullOrWhiteSpace())
            {
                currentName.Remove(currentName.Length - FlagSplitCharacter.Length, FlagSplitCharacter.Length);
            }
            return currentName.ToString();
        }

        private static object BinaryValues(Type enumType, ExpressionType type, object value1, object value2)
        {
            var x = Expression.Parameter(enumType, "x");
            var y = Expression.Parameter(enumType, "y");
            
            var underlyType = Enum.GetUnderlyingType(enumType);

            var left = Expression.Convert(x, underlyType);
            var right = Expression.Convert(y, underlyType);

            var and = Expression.MakeBinary(type, left, right);

            var process = Expression.Lambda(Expression.Convert(and, enumType), x, y);

            return process.Compile().DynamicInvoke(value1, value2);
        }
    }
}
