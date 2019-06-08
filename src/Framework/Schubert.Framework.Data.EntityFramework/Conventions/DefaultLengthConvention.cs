using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Schubert.Framework.Data.Conventions
{
    public class DefaultLengthConvention : IPropertyAddedConvention
    {
        public InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.Metadata.ClrType == typeof(string))
            {
                propertyBuilder.HasMaxLength(32, ConfigurationSource.Convention);
            }

            if (propertyBuilder.Metadata.ClrType == typeof(byte[]))
            {
                propertyBuilder.HasMaxLength(65535, ConfigurationSource.Convention);
            }
            return propertyBuilder;
        }
    }
}
