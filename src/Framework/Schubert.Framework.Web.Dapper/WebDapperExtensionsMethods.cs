using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    public static class WebDapperExtensionsMethods
    {
        public static void InitializeFromClaim(this UserClaim uc, Claim claim)
        {
            uc.ClaimType = claim.Type;
            uc.ClaimValue = claim.Value;
        }

        public static Claim ToClaim(this UserClaim uc)
        {
            return new Claim(uc.ClaimType, uc.ClaimValue);
        }

        public static Claim ToClaim(this RoleClaim rc)
        {
            return new Claim(rc.ClaimType, rc.ClaimValue);
        }
        

        public static void InitializeFromClaim(this RoleClaim rc, Claim other)
        {
            rc.ClaimType = other?.Type;

            rc.ClaimValue = other?.Value;
        }
    }
}
