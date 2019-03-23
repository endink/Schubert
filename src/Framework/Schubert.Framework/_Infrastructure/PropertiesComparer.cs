using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

namespace Schubert
{
    public sealed class PropertiesComparer
    {
        public PropertiesComparer()
        {
            this.RefrencePropertyHandling = RefrencePropertyHandling.PropertyCompare;
            this.CollectionPropertyHandling = CollectionPropertyHandling.ElementPropertyCompare;
        }

        /// <summary>
        /// 引用类型属性比较策略。
        /// </summary>
        public RefrencePropertyHandling RefrencePropertyHandling { get; set; }

        /// <summary>
        /// 集合类型属性比较策略。
        /// </summary>
        public CollectionPropertyHandling CollectionPropertyHandling { get; set; }

        /// <summary>
        /// 搜索属性时的标识。
        /// </summary>
        public BindingFlags PropertyBindingFlags { get; set; }

        private bool CollectionEquals(IEnumerable enum1, IEnumerable enum2, CollectionPropertyHandling handling)
        {
            if(handling == Schubert.CollectionPropertyHandling.Ingore)
            {
                return true;
            }

            bool containsNull = false;
            bool refrenceEquals = PropertiesComparer.NullReferenceEquals(enum1, enum2, out containsNull);
            if (containsNull || refrenceEquals)
            {
                return refrenceEquals;
            }

            Object[] array1 = enum1.Cast<Object>().ToArray();
            Object[] array2 = enum2.Cast<Object>().ToArray();

            if (array1.Length != array2.Length)
            {
                return false;
            }

            switch (handling)
            {
                case Schubert.CollectionPropertyHandling.ElementEqualCompare:
                    for (int i = 0; i < array1.Length; i++)
                    {
                        bool containsNullElement = false;
                        bool elementRefrenceEquals = PropertiesComparer.NullReferenceEquals(array1[i], array2[i], out containsNullElement);
                        if (containsNullElement)
                        {
                            if (!refrenceEquals)
                            {
                                return false;
                            }
                        }
                        else if (!array1[i].Equals(array2[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                case Schubert.CollectionPropertyHandling.ElementPropertyCompare:
                     for (int i = 0; i < array1.Length; i++)
                    {
                        object element1 = array1[i];
                        object element2 = array2[i];
                        bool containsNullElement = false;
                        bool elementRefrenceEquals = PropertiesComparer.NullReferenceEquals(element1, element2, out containsNullElement);
                        if (containsNullElement)
                        {
                            if (!refrenceEquals)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Type type1 = element1.GetType();
                            Type type2 = element2.GetType();
                            type1 = type1.GetTypeInfo().IsAssignableFrom(type2) ? type1 : type2;
                            if (!PropertyEquals(type1, element1, element2))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                case Schubert.CollectionPropertyHandling.EqualCompare:
                    return enum1.Equals(enum2);
                default:
                    return true;
            }
        }

        private bool ObjectEquals(Type propertyType, object value1, object value2, RefrencePropertyHandling handling)
        {
            switch (handling)
            {
                case Schubert.RefrencePropertyHandling.EqualCompare:
                    bool containsNull = false;
                    bool refrenceEquals = PropertiesComparer.NullReferenceEquals(value1, value2, out containsNull);
                    return containsNull || refrenceEquals ? refrenceEquals : value1.Equals(value2);
                case Schubert.RefrencePropertyHandling.PropertyCompare:
                    return this.PropertyEquals(propertyType, value1, value2);
                default:
                    return true;
            }
        }

        private static bool NullReferenceEquals(object object1, object object2, out bool containsNullValue)
        {
            containsNullValue = true;
            if (object1 == null && object2 == null)
            {
                return true;
            }
            else if (object1 == null && object2 != null)
            {
                return false;
            }
            else if (object1 != null && object2 == null)
            {
                return false;
            }
            containsNullValue = false;
            return Object.ReferenceEquals(object1, object2);
        }

        /// <summary>
        /// 对 object1 和 object2 进行属性值比较。
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="object1">要比较的第一个对象。</param>
        /// <param name="object2">要比较的第二个对象。</param>
        /// <param name="ingoreProperties">忽略比较的属性设置。</param>
        /// <returns></returns>
        public bool PropertyEquals<T>(Type objectType, T object1, T object2, Action<ExcludeProperties<T>> ingoreProperties = null)
            where T : class
        {
            ExcludeProperties<T> properties = new ExcludeProperties<T>();
            if (ingoreProperties != null)
            {
                ingoreProperties.Invoke(properties);
            }
            return this.PropertyEquals(typeof(T), object1, object2, properties.PropertyNames.ToArray());
        }

        /// <summary>
        /// 对 object1 和 object2 进行属性值比较。
        /// </summary>
        /// <param name="objectType">要进行属性比较的类型。</param>
        /// <param name="object1">要比较的第一个对象。</param>
        /// <param name="object2">要比较的第二个对象。</param>
        /// <param name="ingorePropertNames">忽略比较的属性名称。</param>
        /// <returns></returns>
        public bool PropertyEquals(Type objectType, object object1, object object2, params string[] ingorePropertNames)
        {
            Guard.ArgumentNotNull(objectType, "objectType");
            Guard.TypeIsAssignableFromType(object1.GetType(), objectType, "object1");
            Guard.TypeIsAssignableFromType(object2.GetType(), objectType, "object2");

            ingorePropertNames = ingorePropertNames ?? new String[0];
            if (objectType.GetTypeInfo().IsValueType)
            {
                return object1.Equals(object2);
            }
            bool containsNull = false;
            bool refrenceEquals = PropertiesComparer.NullReferenceEquals(object1, object2, out containsNull);
            if (containsNull)
            {
                return refrenceEquals;
            }
            else if (refrenceEquals)
            {
                return true;
            }
            else
            {
                PropertyInfo[] infos = objectType.GetTypeInfo().GetProperties(this.PropertyBindingFlags);
                foreach (PropertyInfo property in infos)
                {
                    if (ingorePropertNames.Contains(property.Name))
                    {
                        continue;
                    }
                    ParameterInfo[] index = property.GetIndexParameters();
                    if (index != null && index.Length > 0)
                    {
                        continue;
                    }
                    else
                    {
                        object value1 = property.GetValue(object1, null);
                        object value2 = property.GetValue(object2, null);

                        if (value2 == null && value1 == null)
                        {
                            continue;
                        }

                        if ((value2 == null && value1 != null) || (value2 != null && value1 == null))
                        {
                            return false;
                        }

                        if (!property.PropertyType.GetTypeInfo().IsValueType)
                        {
                            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(property.PropertyType))
                            {
                                if (!CollectionEquals(value1 as IEnumerable, value2 as IEnumerable, this.CollectionPropertyHandling))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (!this.ObjectEquals(property.PropertyType, value1, value2, this.RefrencePropertyHandling))
                                {
                                    return false;
                                }
                            }
                        }
                        if (!value1.Equals(value2))
                        {
                            return false;
                        }

                    }
                }

                return true;
            }
        }

        public sealed class ExcludeProperties<T>
        {
            internal ExcludeProperties ()
	        {
                this.PropertyNames = new Collection<string>();
	        }
            internal Collection<String> PropertyNames {get;set;}

            public ExcludeProperties<T> AddProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
            {
                if (propertyExpression != null)
                {
                    string name = propertyExpression.GetMemberName();
                    if (!this.PropertyNames.Contains(name))
                    {
                        this.PropertyNames.Add(name);
                    }
                }
                return this;
            }
        }
    }

    /// <summary>
    /// 属性比较时对于引用类型的比较策略。
    /// </summary>
    public enum RefrencePropertyHandling
    {
        /// <summary>
        /// 调用 Equal 方法进行比较。
        /// </summary>
        EqualCompare,
        /// <summary>
        /// 比较引用类型的属性（递归）。
        /// </summary>
        PropertyCompare,
        /// <summary>
        /// 忽略引用类型属性的比较。
        /// </summary>
        Ingore,
    }

    /// <summary>
    /// 属性比较时对于可枚举类型的比较策略。
    /// </summary>
    public enum CollectionPropertyHandling
    {
        /// <summary>
        /// 调用 Equal 方法进行比较。
        /// </summary>
        EqualCompare,
        /// <summary>
        /// 对集合中每一个元素 调用 Equal 方法进行比较。
        /// </summary>
        ElementEqualCompare,
        /// <summary>
        /// 比较引用类型的属性（递归）。
        /// </summary>
        ElementPropertyCompare,
        /// <summary>
        /// 忽略集合类型属性的比较。
        /// </summary>
        Ingore,
    }
}
