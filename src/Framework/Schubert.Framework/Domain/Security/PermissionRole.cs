using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Domain
{
    /// <summary>
    /// 表示授权和角色关联。
    /// </summary>
    public class PermissionRole
    {
        /// <summary>
        /// 获取或设置授权 Id。
        /// </summary>
        public long PermissionId { get; set; }

        /// <summary>
        /// 获取或设置角色 Id。
        /// </summary>
        public long RoleId { get; set; }
    }
}
