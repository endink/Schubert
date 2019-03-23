using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Data.Conventions
{
    /// <summary>
    /// 表示一个数据模型的约定。
    /// </summary>
    public class ModelConvention
    {
        private readonly List<PropertyConvention> _properties;
        private readonly List<TypeConvention> _types;
        internal ModelConvention()
        {
            _properties = new List<PropertyConvention>(2);
            _types = new List<TypeConvention>(2);
        }

        internal IList<PropertyConvention> PropertyConventions => _properties;
        internal IList<TypeConvention> TypeConventions => _types;

        /// <summary>
        /// 筛选特定的属性进行约定。
        /// </summary>
        /// <param name="propertyFilter">属性筛选器。</param>
        /// <returns></returns>
        public PropertyConvention Properties(Func<PropertyInfo, bool> propertyFilter)
        {
            Guard.ArgumentNotNull(propertyFilter, nameof(propertyFilter));
            var convention = new PropertyConvention(propertyFilter);
            this._properties.Add(convention);
            return convention;
        }

        /// <summary>
        /// 筛选特定的类型进行约定。
        /// </summary>
        /// <param name="typeFilter">类型筛选器。</param>
        /// <returns></returns> 
        public TypeConvention Types(Func<Type, bool> typeFilter)
        {
            Guard.ArgumentNotNull(typeFilter, nameof(typeFilter));
            var convention = new TypeConvention(typeFilter);
            this._types.Add(convention);
            return convention;
        }
    }


    public static class ModelConventionExtension
    {
        private static bool IsAbstract(Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }
        private static bool IsInterface(Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }
        private static Type[] GetInterfaces(Type type)
        {
            return type.GetTypeInfo().GetInterfaces();
        }
        private static PropertyInfo ReadExpression<T>(Expression<Func<T, object>> expression) where T : class
        {
            var body = expression.Body;
            var unary = body as UnaryExpression;
            if (unary != null) body = unary.Operand;
            var member = body as MemberExpression;
            var property = member?.Member as PropertyInfo;
            if (property == null) throw new NotSupportedException("只接受单个属性表达式,例如 a=>a.Property。");
            return property;
        }
        //public static ModelConvention IsEntity<T>(this ModelConvention convention) where T : class
        //{
        //    var type = typeof(T);
        //    if (IsAbstract(type)) convention.Types(x => x == type).IsEntity();
        //    else if (IsInterface(type)) convention.Types(x => GetInterfaces(x).Any(t => t == type)).IsEntity();
        //    else convention.Types(x => x == type).IsEntity();
        //    return convention;
        //}
        public static ModelConvention IsKey<T>(this ModelConvention convention, Expression<Func<T, object>> expression) where T : class
        {
            var property = ReadExpression(expression);
            convention/*.IsEntity<T>()*/
               .Properties(x => x == property).IsKey();
            return convention;
        }
        public static ModelConvention AutoGeneration<T>(this ModelConvention convention, Expression<Func<T, object>> expression) where T : class
        {
            var property = ReadExpression(expression);
            convention/*.IsEntity<T>()*/
                .Properties(x => x == property).AutoGeneration();
            return convention;
        }
        public static ModelConvention AutoGenerateKey<T>(this ModelConvention convention, Expression<Func<T, object>> expression) where T : class
        {
            var property = ReadExpression(expression);
            convention/*.IsEntity<T>()*/
                .Properties(x => x == property).AutoGenerateKey();
            return convention;
        }
        public static ModelConvention Ignore<T>(this ModelConvention convention, Expression<Func<T, object>> expression) where T : class
        {
            var property = ReadExpression(expression);
            convention/*.IsEntity<T>()*/
                .Properties(x => x == property).Ignore();
            return convention;
        }
    }
}
