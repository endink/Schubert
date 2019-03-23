using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Schubert.Framework.Data.Mappings
{
    internal class ShellDescriptorRecordMapping : EntityMapping<ShellDescriptorRecord>
    {
        public const string TableName = "_ShellDescriptors";

        protected override string StoreTableName
        {
            get
            {
                return TableName;
            }
        }

        protected override void OnApply(EntityTypeBuilder<ShellDescriptorRecord> builder)
        {
            builder.HasKey(r => r.AppName);

            builder.Property(r => r.AppName).HasMaxLength(32);
            builder.Property(r => r.DisabledFeatures).HasMaxLength(4048);
            builder.Property(r => r.Parameters).HasMaxLength(1024);
        }
    }
}
