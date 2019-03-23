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
    public class DapperUserStoreImp : DapperUserStore<UserBase, RoleBase>
    {
        public DapperUserStoreImp(IIdGenerationService idGenerationService, IRepository<UserBase> userRepository, IRepository<RoleBase> roleRepository, IRepository<UserClaim> userClaimRepository, IRepository<UserLogin> userLoginRepository, IRepository<UserToken> userTokenRepository, IRepository<UserRole> userRoleRepository) : base(idGenerationService, userRepository, roleRepository, userClaimRepository, userLoginRepository, userTokenRepository, userRoleRepository)
        {
        }
    }
}
