using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Schubert.Framework.Data.Conventions
{
    public class StringDefaultLengthConvention : IPropertyAddedConvention
    {
        public InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.Metadata.ClrType == typeof(string))
            {
                propertyBuilder.HasMaxLength(32, ConfigurationSource.Convention);
            }
            return propertyBuilder;
        }
    }
}
