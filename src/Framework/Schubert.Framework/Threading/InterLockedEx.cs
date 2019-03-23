using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Threading
{
    public static class InterLockedEx
    {
        /// <summary>
        /// 和 <see cref="Interlocked.CompareExchange(ref int, int, int)"/> 功能相同，区别在于返回是否交换成功。
        /// </summary>
        public static bool CompareAndSet(ref int location1, int value, int comparand)
        {
            int oldValue = Interlocked.CompareExchange(ref location1, value, comparand);
            return oldValue == comparand;
        }

        /// <summary>
        /// 和 <see cref="Interlocked.CompareExchange(ref long, long, long)"/> 功能相同，区别在于返回是否交换成功。
        /// </summary>
        public static bool CompareAndSet(ref long location1, long value, long comparand)
        {
            long oldValue = Interlocked.CompareExchange(ref location1, value, comparand);
            return oldValue == comparand;
        }
    }
}
