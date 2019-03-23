using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schubert.Framework.Domain;

namespace Schubert.Framework.Data.Mappings
{
    public class LanguageMapping : EntityMapping<Language>
    {
        public const string TableName = "Languages";

        protected override string StoreTableName
        {
            get
            {
                return TableName;
            }
        }

        protected override void OnApply(EntityTypeBuilder<Language> builder)
        {
            builder.HasKey(l => l.Culture);
            builder.Property(l => l.Culture).HasMaxLength(32);
            builder.Property(l => l.DisplayName).HasMaxLength(32);
            builder.Property(l => l.UniqueSeoCode).HasMaxLength(32);
            builder.Property(l => l.FlagImageFileName).HasMaxLength(256);

            builder.HasMany(l => l.StringResources).WithOne().HasForeignKey(s => s.Culture);
        }
    }
}
