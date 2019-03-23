using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Schubert.Framework.Data.Conventions;
using JetBrains.Annotations;

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
            set.PropertyAddedConventions.Insert(0, new StringDefaultLengthConvention());
            
            return set;
        }

        
    }
}
