using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Schubert.Helpers;

namespace Schubert
{
    public class PropertyComparer<TSource> : IEqualityComparer<TSource>
    {
        private Delegate m_propertyGetter = null;

        public PropertyComparer(string propertyPath)
        {
            Guard.ArgumentNullOrWhiteSpaceString(propertyPath, "propertyName");
            this.m_propertyGetter = ExpressionHelper.MakePropertyLambda(typeof(TSource), propertyPath).Compile();
        }

        public bool Equals(TSource x, TSource y)
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
                object xProperty = this.m_propertyGetter.DynamicInvoke(x);
                object yProperty = this.m_propertyGetter.DynamicInvoke(y);

                if (xProperty == null && yProperty == null)
                {
                    return true;
                }
                else if ((xProperty == null && yProperty != null) || (xProperty != null && yProperty == null))
                {
                    return false;
                }
                else
                {
                    return xProperty.Equals(yProperty);
                }
            }
        }


        public int GetHashCode(TSource obj)
        {
            Guard.ArgumentNotNull(obj, "obj");
            object xProperty = this.m_propertyGetter.DynamicInvoke(obj);
            return xProperty == null ? int.MinValue : xProperty.GetHashCode();
        }
    }
}
