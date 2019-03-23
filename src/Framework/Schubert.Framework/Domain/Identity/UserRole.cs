using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Domain
{
    public class UserRole
    {
        public long UserId { get; set; }

        public long RoleId { get; set; }

        public string RoleName { get; set; }
    }
}
