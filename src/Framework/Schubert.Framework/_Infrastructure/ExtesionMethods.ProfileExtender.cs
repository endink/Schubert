using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Schubert;

namespace System
{
    partial class ExtensionMethods
    {
        public static T GetProfileProperty<T>(this ProfileExtender extender, string name)
        {
            dynamic s = extender.GetProfileProperty(name) as ConvertibleString;
            return s;
        }
    }
}
