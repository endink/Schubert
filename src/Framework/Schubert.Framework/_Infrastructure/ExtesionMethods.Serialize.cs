using Schubert.Helpers;

namespace System
{
    static partial class ExtensionMethods
    {
        public static string XmlSerialize<T>(this T obj)
        {
            return ToolHelper.XmlSerialize<T>(obj);
        }
    }
}
