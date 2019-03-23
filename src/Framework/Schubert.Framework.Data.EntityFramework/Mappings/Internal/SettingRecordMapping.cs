using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Schubert.Framework.Data.Mappings
{
    public class SettingRecordMapping : EntityMapping<SettingRecord>
    {
        public const string TableName = "_Configurations";
        protected override string StoreTableName
        {
            get
            {
                return TableName;
            }
        }

        protected override void OnApply(EntityTypeBuilder<SettingRecord> builder)
        {
            builder.HasKey(r => r.Id);

            //builder.HasIndex(r => new { r.Id, r.Name, r.Region }).IsUnique();
            
            builder.Property(r => r.Name).IsRequired().HasMaxLength(256);
            builder.Property(r => r.Region).IsRequired().HasMaxLength(64);
            builder.Property(r => r.RawValue).HasMaxLength(int.MaxValue);
            builder.Property(r => r.Id).HasMaxLength(40);
        }
    }
}
