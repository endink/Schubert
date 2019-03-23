using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Schubert.Framework.Data;
using Schubert.Framework.Domain;
using System.Security.Claims;
using Schubert.Framework.Services;

namespace Schubert.Framework.Web.AspNetIdentity
{
    public class DapperRoleStore<TRole> : IRoleStore<TRole>, IRoleClaimStore<TRole>
        where TRole : RoleBase
    {
        private IRepository<TRole> _roleRepository;
        private IRepository<RoleClaim> _roleClaimRepository;
        private IIdGenerationService _idGenerationService = null;
        private bool _disposed = false;

        public DapperRoleStore(
            IIdGenerationService idGenerationService,
            IRepository<TRole> roleRepository,
            IRepository<RoleClaim> roleClaimRepository)
        {
            Guard.ArgumentNotNull(idGenerationService, nameof(idGenerationService));
            Guard.ArgumentNotNull(roleRepository, nameof(roleRepository));
            Guard.ArgumentNotNull(roleClaimRepository, nameof(roleClaimRepository));

            _idGenerationService = idGenerationService;
            this._roleRepository = roleRepository;
            this._roleClaimRepository = roleClaimRepository;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }

        protected virtual RoleClaim CreateRoleClaim(TRole role, Claim claim)
        {
            return new RoleClaim { Id = _idGenerationService.GenerateId()  , RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value };
        }

        #region IRoleStore

        public virtual async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(role, nameof(role));

            await _roleRepository.InsertAsync(role);
            return IdentityResult.Success;
        }

        public virtual async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(role, nameof(role));

            await _roleRepository.DeleteAsync(role);
            return IdentityResult.Success;
        }

        public virtual async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            long longId;
            if (long.TryParse(roleId, out longId))
            {
                SingleQueryFilter filter = new SingleQueryFilter();
                filter.AddEqual(nameof(RoleBase.Id), longId);
                return await _roleRepository.QueryFirstOrDefaultAsync(filter);
            }
            else
            {
                return null;
            }
        }

        public virtual async Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(RoleBase.NormalizedName), normalizedRoleName);
            return await _roleRepository.QueryFirstOrDefaultAsync(filter);
        }

        public virtual Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(role, nameof(role));

            return Task.FromResult(role.NormalizedName);
        }

        public virtual Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return Task.FromResult(role.Id.ToString());
        }

        public virtual Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return Task.FromResult(role.Name);
        }

        public virtual Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public virtual async Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNullOrWhiteSpaceString(roleName, nameof(roleName));
            Guard.ArgumentNotNull(role, nameof(role));
            if (role.Name.CaseSensitiveEquals(roleName))
            {
                role.Name = roleName;
            }
            Dictionary<String, Object> fieldsToUpdate = new Dictionary<string, object>();
            fieldsToUpdate.Add(nameof(RoleBase.Name), roleName);

            await _roleRepository.UpdateAsync(role, fieldsToUpdate);
        }

        public virtual async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(role, nameof(role));
            await _roleRepository.UpdateAsync(role);
            return IdentityResult.Success;
        }

        #endregion

        public virtual async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(role, nameof(role));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(RoleBase.Id), role.Id);
            var roleClaims = await _roleClaimRepository.QueryAsync(filter);
            return roleClaims.Select(r => r.ToClaim()).ToArray();
        }

        public virtual Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(role, nameof(role));
            Guard.ArgumentNotNull(claim, nameof(claim));

            var roleClaim = this.CreateRoleClaim(role, claim);
            return _roleClaimRepository.InsertAsync(roleClaim);
        }

        public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(role, nameof(role));
            Guard.ArgumentNotNull(claim, nameof(claim));
            
            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(RoleClaim.RoleId), role.Id);
            filter.AddEqual(nameof(RoleClaim.ClaimType), claim.Type);
            filter.AddEqual(nameof(RoleClaim.ClaimValue), claim.Value);
            return _roleClaimRepository.DeleteAsync(filter);
        }
    }
}
