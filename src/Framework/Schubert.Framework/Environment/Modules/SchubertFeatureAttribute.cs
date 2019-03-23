using System;

namespace Schubert.Framework.Environment.Modules
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SchubertFeatureAttribute : Attribute
    {
        public SchubertFeatureAttribute(string featureName)
        {
            this.FeatureName = featureName;
        }

        public string FeatureName { get; set; }
    }
}