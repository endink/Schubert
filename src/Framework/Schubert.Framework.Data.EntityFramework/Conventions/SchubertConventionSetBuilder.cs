using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Schubert.Framework.Data.Conventions;

namespace Schubert.Framework.Data.Providers
{
    class SchubertConventionSetBuilder : CoreConventionSetBuilder
    {
        public SchubertConventionSetBuilder(CoreConventionSetBuilderDependencies dependencies) 
            : base(dependencies)
        {
        }

        public override ConventionSet CreateConventionSet()
        {
            var set = base.CreateConventionSet();
            set.PropertyAddedConventions.Insert(0, new DefaultLengthConvention());
            
            return set;
        }

        
    }
}
