/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2008-04-27 13:26:43 
===============================================================================
 Copyright ? Sharping.  All rights reserved.
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
 OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
 LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 FITNESS FOR A PARTICULAR PURPOSE.
===============================================================================
*/

using System.ComponentModel;
using System.Reflection;
using Schubert;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using Schubert.Helpers;

namespace System
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class ExtensionMethods
    {
        public static object DefaultValue(this Type type)
        {
            var expression = Expression.Default(type);
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        public static T If<T>(this T item, Func<T, bool> predicate, T returnValue)
        {
            if (predicate(item))
            {
                return returnValue;
            }
            return item;
        }

        #region ValueType

        public static T IfDefault<T>(this T item, T @default)
            where T : struct, IComparable<T>
        {
            if (item.CompareTo(default(T)) == 0)
            {
                return @default;
            }
            return item;
        }

        public static Nullable<T> IfDefaultReturnNull<T>(this T item)
            where T : struct, IComparable<T>
        {
            return item.CompareTo(default(T)) == 0 ? null : new Nullable<T>(item);
        }

        public static T IfNullReturnDefault<T>(this Nullable<T> item)
            where T : struct
        {
            return item ?? default(T);
        }

        public static T IfNull<T>(this T? value, T defaultValue)
            where T :struct
        {
            return value.HasValue ? value.Value : defaultValue;
        }

        #endregion

        #region Object

        public static dynamic Reflect(this object instance)
        {
            if (instance == null)
            {
                return null;
            }
            else
            {
                return new ReflectionObject(instance);
            }
        }

        public static bool SafeEquals(this object object1, object object2)
        {
            if (object1 == null && object2 == null)
            {
                return true;
            }
            else if (object1 == null && object2 != null)
            {
                return false;
            }
            else if (object2 == null && object1 != null)
            {
                return false;
            }
            else
            {
                return object1.Equals(object2);
            }
        }


        public static T IfNull<T>(this T item, T @default)
            where T : class
        {
            if (item == null)
            {
                return @default;
            }
            return item;
        }

        private static bool PropertiesEquals(Type compareType, object x, object y, BindingFlags propertyFlags = BindingFlags.Public | BindingFlags.Instance, bool recurseComplexTypeProperties = true, Func<PropertyInfo, bool> predicate = null)
        {
            if ((x == null && y == null) || Object.ReferenceEquals(x, y))
            {
                return true;
            }
            if (x != null && y != null)
            {
                PropertyInfo[] infos = compareType.GetTypeInfo().GetProperties(propertyFlags);
                foreach (PropertyInfo property in infos)
                {
                    ParameterInfo[] index = property.GetIndexParameters();
                    if (index != null && index.Length > 0)
                    {
                        continue;
                    }

                    if (predicate != null && !predicate.Invoke(property))
                    {
                        continue;
                    }

                    object value1 = property.GetValue(x, null);
                    object value = property.GetValue(y, null);
                    bool equals = value1.SafeEquals(value);

                    if (value == null && value1 == null)
                    {
                        continue;
                    }

                    if ((value == null && value1 != null) || (value != null && value1 == null))
                    {
                        return false;
                    }

                    if (!value1.Equals(value))
                    {
                        if (!property.PropertyType.CanUseForDb() && recurseComplexTypeProperties)
                        {
                            if (!PropertiesEquals(property.PropertyType, value1, value, propertyFlags, recurseComplexTypeProperties, predicate))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public static bool ArraysEqual<T>(this T[] a1, T[] a2)
        {
            //also see Enumerable.SequenceEqual(a1, a2);
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// 比较两个实体属性值是否相等(不比较索引器)。
        /// </summary>
        /// <param name="thisObject"></param>
        /// <param name="obj">要比较的实体,可以是派生类型</param>
        /// <param name="propertyFlags">属性的一个或多个搜索执行方式</param>
        /// <param name="predicate">属性比较筛选器，如果不为空，必须满足删选器条件的属性才进行比较。</param>
        /// <param name="recurseComplexTypeProperties">是否递归复合类型属性。</param>
        public static bool PropertiesEquals(this object thisObject, object obj, BindingFlags propertyFlags = BindingFlags.Public | BindingFlags.Instance, bool recurseComplexTypeProperties = true, Func<PropertyInfo, bool> predicate = null)
        {
            if ((thisObject == null && obj == null) || Object.ReferenceEquals(thisObject, obj))
            {
                return true;
            }
            if (thisObject != null && obj != null)
            {
                Type declareType = thisObject.GetType();
                Type type = obj.GetType();
                if (!declareType.GetTypeInfo().IsAssignableFrom(type))
                {
                    return false;
                }
                return PropertiesEquals(declareType, thisObject, obj, propertyFlags, recurseComplexTypeProperties, predicate);
            }
            return false;
        }

        public static bool ContainsAttributes(this PropertyInfo info, Type[] attributeTypes, bool inherit)
        {
            if (attributeTypes != null && attributeTypes.Length > 0)
            {
                foreach (Type type in attributeTypes)
                {
                    
                    if (typeof(Attribute).GetTypeInfo().IsAssignableFrom(type))
                    {
                        if ((info.GetCustomAttributes(type, inherit).Count()) == 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("the item of attributeTypes must be a type of {0}", typeof(Attribute).AssemblyQualifiedName));
                    }
                }
                return true;
            }
            else
            {
                throw new ArgumentException("the attributeTypes cant be null or empty array");
            }
        }

        #endregion

        public static string GetDescription(this Enum enumValue)
        {
            return EnumDescriptionAttribute.GetDescription(enumValue);
        }

        /// <summary>
        /// 将匿名对象 （new { Propert1 = XXX, Property2 = XXX }）转换为 <see cref="IDictionary{String,Object}"/> 字典。
        /// </summary>
        /// <param name="obj">要转换的匿名对象。</param>
        /// <returns>如果 <paramref name="obj"/> 实现了 <see cref="IDictionary{String,Object}"/>接口， 
        /// 直接将对象转换为 <see cref="IDictionary{String,Object}"/> 后返回。如果对象为 null， 返回 null。
        /// 否则返回一个 <see cref="System.Collections.Hashtable"/> ，包含所有公共实例属性的名称和值的字典。
        /// </returns>
        public static IDictionary<String, Object> ToDictionary(this object obj)
        {
            return DictionaryHelper.Convert(obj);
        }

    }
}
