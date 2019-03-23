using Schubert.Framework.Domain;
using Schubert.Framework.Web.AspNetIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.Data;
using Schubert.Framework.Services;

namespace Schubert.Framework.Test.DapperIdentity
{
    public class DapperRoleStoreImp : DapperRoleStore<RoleBase>
    {
        public DapperRoleStoreImp(IIdGenerationService idGenerationService, IRepository<RoleBase> roleRepository, IRepository<RoleClaim> roleClaimRepository) : base(idGenerationService, roleRepository, roleClaimRepository)
        {
        }
    }
}
