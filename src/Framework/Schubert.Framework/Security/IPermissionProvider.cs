using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Security
{
    /// <summary>
    /// <see cref="Permission"/> 提供程序。
    /// </summary>
    public interface IPermissionProvider : ITransientDependency
    {
        /// <summary>
        /// 获取授权集合。
        /// </summary>
        /// <returns>一个 <see cref="Permission"/> 集合。</returns>
        IEnumerable<Permission> GetPermissions();
    }
}
