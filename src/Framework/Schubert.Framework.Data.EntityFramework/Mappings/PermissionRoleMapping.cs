using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schubert.Framework.Domain;

namespace Schubert.Framework.Data.Mappings
{
    public class PermissionRoleMapping : EntityMapping<PermissionRole>
    {
        public const string TableName = "Permissions_Roles";

        protected override string StoreTableName
        {
            get
            {
                return TableName;
            }
        }

        protected override void OnApply(EntityTypeBuilder<PermissionRole> builder)
        {
            builder.HasKey(p => new { p.PermissionId, p.RoleId });
        }
    }
}
