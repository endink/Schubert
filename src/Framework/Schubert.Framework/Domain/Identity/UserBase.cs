using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Domain
{
    public class UserBase : IdentityUser<long>
    {
        public UserBase()
        {
            this.SecurityStamp = Guid.NewGuid().ToString("N");
        }

        public string Language { get; set; } = "zh-Hans";

        public string TimeZone { get; set; } = "Asia/Shanghai";
    }
}
