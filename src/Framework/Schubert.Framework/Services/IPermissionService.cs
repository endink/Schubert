using Schubert.Framework.Domain;
using Schubert.Framework.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Services
{
    public interface IPermissionService : IDependency
    {
        /// <summary>
        /// 删除授权（表示删除授权和角色的关系）。
        /// </summary>
        /// <param name="permission">Permission</param>
        Task DeletePermissionAsync(Permission permission);

        /// <summary>
        /// 根据系统名称获取一个授权对象。
        /// </summary>
        /// <param name="permissionName">授权对象系统名称。</param>
        /// <returns><see cref="Permission"/> 对象。</returns>
        Task<Permission> GetPermissionBySystemNameAsync(string permissionName);

        /// <summary>
        /// 根据授权 Id 获取授权信息。
        /// </summary>
        /// <param name="permissionId">授权 Id。</param>
        /// <returns>返回 <see cref="Permission"/> 对象。</returns>
        Task<Permission> GetPermissionByIdAsync(long permissionId);

        /// <summary>
        /// 获取所有的授权对象。
        /// </summary>
        /// <returns>授权对象集合。</returns>
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();

        /// <summary>
        /// 创建授权（表示添加授权和角色的关系）。
        /// </summary>
        /// <param name="permission"><see cref="Permission"/> 对象实例。</param>
        Task CreatePermissionAsync(Permission permission);

        /// <summary>
        /// 更新一个授权对象。
        /// </summary>
        /// <param name="permission">授权对象实例。</param>
        Task UpdatePermissionAsync(Permission permission);

        /// <summary>
        /// 安装授权提供程序中的授权信息。
        /// </summary>
        /// <param name="permissionProvider">授权提供程序。</param>
        Task InstallPermissionsAsync(IPermissionProvider permissionProvider);

        /// <summary>
        /// 卸载授权提供程序中的信息。
        /// </summary>
        /// <param name="permissionProvider">授权提供程序。</param>
        Task UninstallPermissionsAsync(IPermissionProvider permissionProvider);

        /// <summary>
        /// 判断用户是否具有授权。
        /// </summary>
        /// <param name="permissionName">授权的系统名称。</param>
        /// <param name="userId">用户 Id。</param>
        Task<bool> HasPermissionAsync(string permissionName, long userId);

        //Task<IEnumerable<String>> GetUserPermissionsAsync(int userId);
    }
}
