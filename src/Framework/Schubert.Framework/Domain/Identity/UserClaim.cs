using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Domain
{
    public class UserClaim
    {
        public long Id { get; set; }

        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }

        public long UserId { get; set; }
    }
}
