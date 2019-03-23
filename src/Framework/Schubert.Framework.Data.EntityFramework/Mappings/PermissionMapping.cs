using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schubert.Framework.Domain;

namespace Schubert.Framework.Data.Mappings
{
    public class PermissionMapping : EntityMapping<Permission>
    {
        public const string TableName = "Permissions";

        protected override string StoreTableName
        {
            get
            {
                return TableName;
            }
        }

        protected override void OnApply(EntityTypeBuilder<Permission> typeBuilder)
        {
            typeBuilder.HasKey(p => p.Id);

            typeBuilder.Property(p => p.Id).ValueGeneratedNever();
            typeBuilder.Property(p => p.Name).HasMaxLength(32).IsRequired();
            typeBuilder.Property(p => p.DisplayName).HasMaxLength(32).IsRequired();
            typeBuilder.Property(p => p.Discription).HasMaxLength(512);
            typeBuilder.Property(p => p.Category).HasMaxLength(64);
            typeBuilder.HasMany(p => p.Roles).WithOne().HasForeignKey(pr=>pr.PermissionId);
        }
    }
}
