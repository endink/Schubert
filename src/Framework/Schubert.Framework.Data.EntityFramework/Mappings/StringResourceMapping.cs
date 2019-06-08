using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schubert.Framework.Domain;

namespace Schubert.Framework.Data.Mappings
{
    public class StringResourceMapping : EntityMapping<StringResource>
    {
        public const string TableName = "StringResources";

        protected override string StoreTableName
        {
            get
            {
                return TableName;
            }
        }

        protected override void OnApply(EntityTypeBuilder<StringResource> builder)
        {
            builder.HasKey(s => new { s.Culture, s.ResourceName });

            builder.Property(r => r.Culture).HasMaxLength(32);
            builder.Property(r => r.ResourceName).HasMaxLength(64);
            builder.Property(r => r.ResourceValue).HasMaxLength(65535, this.DbProvider).IsRequired();
        }
    }
}
