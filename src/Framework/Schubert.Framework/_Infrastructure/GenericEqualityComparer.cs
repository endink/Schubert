using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schubert
{
    public class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> m_comparerAction = null;

        public GenericEqualityComparer(Func<T, T, bool> comparerAction)
        {
            Guard.ArgumentNotNull(comparerAction, "comparerAction");
            this.m_comparerAction = comparerAction;
        }

        public bool Equals(T x, T y)
        {
             if (x == null && y == null)
            {
                return true;
            }
             else if ((x == null && y != null) || (x != null && y == null))
             {
                 return false;
             }
             else
             {
                 return this.m_comparerAction.Invoke(x, y);
             }
        }

        public int GetHashCode(T obj)
        {
            return this.m_comparerAction.GetHashCode();
        }
    }
}
