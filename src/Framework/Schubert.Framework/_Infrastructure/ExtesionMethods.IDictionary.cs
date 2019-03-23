using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Schubert;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    partial class ExtensionMethods
    {
        /// <summary>
        /// 合并字典。
        /// </summary>
        /// <param name="instance">源字典。</param>
        /// <param name="toAdd">要合并的字典。</param>
        /// <param name="replaceExisting">如果为 <c>true</c>，将替换已存在的项，如果为 false, 跳过已存在的项.</param>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> instance, IEnumerable<KeyValuePair<TKey, TValue>> toAdd, bool replaceExisting = true)
        {
            Guard.ArgumentNotNull(toAdd, nameof(toAdd));
            
            foreach (KeyValuePair<TKey, TValue> pair in toAdd)
            {
                instance.Set(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// 仅在字典中不存在键时候插入键和值，如果存在不进行插入。
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key">要插入的键。</param>
        /// <param name="value">要插入的值。</param>
        /// <returns>如果插入了值，返回 true；否则，返回 false。</returns>
        public static bool AddIfNotExisting<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key))
            {
                return false;
            }
            dic.Add(key, value);
            {
                return true;
            }
        }

        public static TValue RemoveAndGet<TKey, TValue>(this IDictionary<TKey, TValue> instance, TKey key)
        {
            TValue result;
            if (instance.TryGetValue(key, out result))
            {
                if (instance.Remove(key))
                {
                    return result;
                }
            }
            return default(TValue);
        }

        /// <summary>
        /// 获取字典中的指定键的值，如果指定的键不存在，则返回 <paramref name="valueFactory"/> 委托的返回值 。
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> instance, TKey key, Func<TValue> valueFactory)
        {
            Guard.ArgumentNotNull(valueFactory, nameof(valueFactory));
            TValue result;
            if (!instance.TryGetValue(key, out result))
            {
                return valueFactory.Invoke();
            }

            return result;
        }

        /// <summary>
        /// 获取字典中的指定键的值，如果指定的键不存在，则返回 <paramref name="defaultValue"/> 。
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> instance, TKey key, TValue defaultValue = default(TValue))
        {
            TValue result;
            if (!instance.TryGetValue(key, out result))
            {
                return defaultValue;
            }

            return result;
        }

        /// <summary>
        /// 如果指定的键尚不存在，则将键/值对添加到字典中。
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> instance, TKey key, Func<TKey, TValue> valueFunc)
        {
            Guard.ArgumentNotNull(valueFunc, nameof(valueFunc));

            TValue result;
            if (!instance.TryGetValue(key, out result))
            {
                result = valueFunc(key);
                instance.Add(key, result);
            }

            return result;
        }

        /// <summary>
        /// 如果指定的键尚不存在，则将键/值对添加到字典中。
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, Object> instance, TKey key, Func<TKey, TValue> valueFunc)
        {
            Guard.ArgumentNotNull(valueFunc, nameof(valueFunc));

            object result;
            if (!instance.TryGetValue(key, out result))
            {
                result = valueFunc(key);
                instance.Add(key, result);
            }

            return (TValue)result;
        }

        public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey attribute, TValue value, bool replaceExisting = true)
        {
            if (dic.ContainsKey(attribute))
            {
                if (replaceExisting)
                {
                    dic[attribute] = value;
                }
            }
            else
            {
                dic.Add(attribute, value);
            }
        }

        public static IDictionary<String, Object> AttachPrefiex(this IDictionary<String, Object> values, string prefix)
        {
            if (prefix.IsNullOrWhiteSpace())
            {
                return values;
            }

            var newValues = values.Select(kp => new KeyValuePair<String, object>(String.Format("{0}.{1}", prefix, kp.Key), kp.Value)).ToArray();
            values.Clear();
            foreach (var value in newValues)
            {
                values.Set(value.Key, value.Value);
            }
            return values;
        }
        


        public static void AddValues(this IDictionary<String, Object> instance, object values, bool replaceExisting = true)
        {
            if (values != null)
            {
                IDictionary<String, Object> dic = values.ToDictionary();
                instance.AddRange(dic, replaceExisting);
            }
        }

        public static void AddValues(this IDictionary<String, String> instance, object values, bool replaceExisting = true)
        {
            if (values != null)
            {
                var keyValues = values.ToDictionary().Where(kp=>kp.Value != null).Select(kp=>new KeyValuePair<String, String>(kp.Key, kp.Value.ToString()));
                instance.AddRange(keyValues, replaceExisting);
            }
        }

        public static TValue Get<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary,
                TKey key,
                TValue defaultValue = default(TValue))
        {
            TValue v = default(TValue);
            if (dictionary.TryGetValue(key, out v))
            {
                return v;
            }
            return defaultValue;
        }

        public static TValue SafeGetValue<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary,
                TKey key,
                object dictionaryLock,
                Func<TValue> valueInitializer)
            {
                TValue value;
                var found = dictionary.TryGetValue(key, out value);
                if (found) return value;

                lock (dictionaryLock)
                {
                    found = dictionary.TryGetValue(key, out value);
                    if (found) return value;

                    value = valueInitializer();

                    dictionary.Add(key, value);
                }
                return value;
            }

        public static async Task<TValue> SafeGetValueAsync<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            object syncObject,
            ManualResetEventSlim syncLock,
            Func<TKey, Task<TValue>> valueFunc)
        {
            TValue value;
            var found = dictionary.TryGetValue(key, out value);
            if (found)
            {
                return value;
            }

            lock (syncObject)
            {
                syncLock.Wait();
                syncLock.Reset();
            }
            found = dictionary.TryGetValue(key, out value);

            try
            {
                if (!found)
                {
                    value = await valueFunc(key);
                    dictionary.Add(key, value);
                }
            }
            finally
            {
                syncLock.Set();
            }

            return value;
        }
    }
}
