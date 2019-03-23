using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Domain
{
    /// <summary>
    /// 表示一个授权对象。
    /// </summary>
    public class Permission
    {
        private ICollection<PermissionRole> _roles;

        public long Id { get; set; }
        
        /// <summary>
        /// 获取或设置授权的系统名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置授权的显示名称。
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// 获取或设置授权分类。
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 获取或设置权限描述。
        /// </summary>
        public string Discription { get; set; }

        /// <summary>
        /// 获取或设置授权中的角色。
        /// </summary>
        public ICollection<PermissionRole> Roles
        {
            get { return _roles ?? (_roles = new List<PermissionRole>()); }
            protected set { _roles = value; }
        }

        public override string ToString()
        {
            return $"Permission : {this.Name}";
        }
    }
}
