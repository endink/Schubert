using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Threading;

namespace Schubert.Helpers
{
    public static class DictionaryHelper
    {
        public static Func<object, IDictionary<string, object>> CreateObjectToDictionaryConverter(Type itemType)
        {
            Guard.ArgumentNotNull(itemType, nameof(itemType));

            var dictType = typeof(Dictionary<string, object>);
            // setup dynamic method
            // Important: make itemType owner of the method to allow access to internal types
            var dm = new DynamicMethod(string.Empty, typeof(IDictionary<string, object>), new[] { typeof(object) }, itemType);
            var il = dm.GetILGenerator();

            // Dictionary.Add(object key, object value)
            var addMethod = dictType.GetTypeInfo().GetMethod("Add");

            // create the Dictionary and store it in a local variable
            il.DeclareLocal(dictType);
            il.Emit(OpCodes.Newobj, dictType.GetTypeInfo().GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);

            var attributes = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            foreach (var property in itemType.GetTypeInfo().GetProperties(attributes).Where(info => info.CanRead))
            {
                // load Dictionary (prepare for call later)
                il.Emit(OpCodes.Ldloc_0);
                // load key, i.e. name of the property
                il.Emit(OpCodes.Ldstr, property.Name);

                // load value of property to stack
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, property.GetGetMethod(), null);
                // perform boxing if necessary
                if (property.PropertyType.GetTypeInfo().IsValueType)
                {
                    il.Emit(OpCodes.Box, property.PropertyType);
                }
                // stack at this point
                // 1. string or null (value)
                // 2. string (key)
                // 3. dictionary
                // ready to call dict.Add(key, value)
                il.EmitCall(OpCodes.Callvirt, addMethod, null);
            }
            // finally load Dictionary and return
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return (Func<object, IDictionary<string, object>>)dm.CreateDelegate(typeof(Func<object, IDictionary<string, object>>));
        }
        

        /// <summary>
        /// 将匿名对象 （new { Propert1 = XXX, Property2 = XXX }）转换为 <see cref="IDictionary{String,Object}"/> 字典。
        /// </summary>
        /// <param name="dataObject">要转换的匿名对象。</param>
        /// <returns>如果 <paramref name="dataObject"/> 实现了 <see cref="IDictionary{String,Object}"/>接口， 
        /// 直接将对象转换为 <see cref="IDictionary{String,Object}"/> 后返回。
        /// 否则返回一个 <see cref="System.Collections.Hashtable"/> ，包含所有公共实例属性的名称和值的字典。
        /// </returns>
        public static IDictionary<string, object> Convert(object dataObject)
        {
            if (dataObject == null)
            {
                return new Dictionary<String, Object>();
            }
            if (dataObject is IDictionary<string, object>)
            {
                return (IDictionary<string, object>)dataObject;
            }
            return CreateObjectToDictionaryConverter(dataObject.GetType())(dataObject);
        }

    }
}