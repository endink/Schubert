using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.Domain;
using Schubert.Framework.Security;

namespace Schubert.Framework.Services
{
    
    public class NullPermissionService : IPermissionService
    {
        public Task CreatePermissionAsync(Permission permission)
        {
            return Task.FromResult(0);
        }

        public Task DeletePermissionAsync(Permission permission)
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return Task.FromResult(Enumerable.Empty<Permission>());
        }

        public Task<Permission> GetPermissionByIdAsync(long permissionId)
        {
            return Task.FromResult<Permission>(null);
        }

        public Task<Permission> GetPermissionBySystemNameAsync(string name)
        {
            return Task.FromResult<Permission>(null);
        }

        public Task<bool> HasPermissionAsync(string permissionSystemName, long userId)
        {
            return Task.FromResult(false) ;
        }

        public Task InstallPermissionsAsync(IPermissionProvider permissionProvider)
        {
            return Task.FromResult(0);
        }

        public Task UninstallPermissionsAsync(IPermissionProvider permissionProvider)
        {
            return Task.FromResult(0);
        }

        public Task UpdatePermissionAsync(Permission permission)
        {
            return Task.FromResult(0);
        }
    }
}
