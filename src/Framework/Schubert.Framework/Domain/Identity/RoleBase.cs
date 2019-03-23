using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Domain
{
    public class RoleBase : IdentityRole<long>
    {
        public string DisplayName { get; set; }
    }
}
