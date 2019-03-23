using Microsoft.AspNetCore.Identity;
using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Schubert.Framework.Data;
using System.Security.Claims;
using Schubert.Framework.Services;

namespace Schubert.Framework.Web.AspNetIdentity
{
    public class DapperUserStore<TUser, TRole> : 
        IUserStore<TUser>, 
        IUserRoleStore<TUser>, 
        IUserEmailStore<TUser>, 
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IUserTwoFactorStore<TUser>,
        IUserAuthenticationTokenStore<TUser>
        where TUser : UserBase
        where TRole : RoleBase
    {
        private IRepository<TUser> _userRepository = null;
        private IRepository<UserClaim> _userClaimRepository = null;
        private IRepository<UserLogin> _userLoginRepository = null;
        private IRepository<UserToken> _userTokenRepository = null;
        private IIdGenerationService _idGenerationService = null;
        private IRepository<TRole> _roleRepository = null;
        private IRepository<UserRole> _userRoleRepository = null;
        private bool _disposed = false;

        public DapperUserStore(
            IIdGenerationService idGenerationService,
            IRepository<TUser> userRepository,
            IRepository<TRole> roleRepository,
            IRepository<UserClaim> userClaimRepository,
            IRepository<UserLogin> userLoginRepository,
            IRepository<UserToken> userTokenRepository,
            IRepository<UserRole> userRoleRepository)
        {
            Guard.ArgumentNotNull(idGenerationService, nameof(idGenerationService));
            Guard.ArgumentNotNull(userRepository, nameof(userRepository));
            Guard.ArgumentNotNull(roleRepository, nameof(roleRepository));
            Guard.ArgumentNotNull(userLoginRepository, nameof(userLoginRepository));
            Guard.ArgumentNotNull(userClaimRepository, nameof(userClaimRepository));
            Guard.ArgumentNotNull(userTokenRepository, nameof(userTokenRepository));
            Guard.ArgumentNotNull(userRoleRepository, nameof(userRoleRepository));

            _idGenerationService = idGenerationService;
            _userRepository = userRepository;
            _userLoginRepository = userLoginRepository;
            _userClaimRepository = userClaimRepository;
            _userTokenRepository = userTokenRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        #region IDispose

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

        #endregion

        #region IUserStore

        public virtual async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            await _userRepository.InsertAsync(user);
            return IdentityResult.Success;
        }

        public virtual async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            await _userRepository.DeleteAsync(user);
            return IdentityResult.Success;
        }

        public virtual async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            long longId;
            if (long.TryParse(userId, out longId))
            {
                SingleQueryFilter filter = new SingleQueryFilter();
                filter.AddEqual(nameof(UserBase.Id), longId);
                return await _userRepository.QueryFirstOrDefaultAsync(filter);
            }
            else
            {
                return null;
            }
        }

        public virtual async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (normalizedUserName.IsNullOrWhiteSpace())
            {
                return null;
            }
            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserBase.UserName), normalizedUserName);
            return await _userRepository.QueryFirstOrDefaultAsync(filter);
        }

        public virtual Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.NormalizedUserName);
        }

        public virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.Id.ToString());
        }

        public virtual Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.UserName);
        }

        public virtual Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNullOrWhiteSpaceString(normalizedName, nameof(normalizedName));
            if (!user.NormalizedUserName.CaseInsensitiveEquals(normalizedName))
            {
                user.NormalizedUserName = normalizedName;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.NormalizedUserName), normalizedName.ToLower());
            return _userRepository.UpdateAsync(user, parameters);
        }

        public virtual async Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            if (!user.UserName.CaseSensitiveEquals(userName))
            {
                user.UserName = userName;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.UserName), userName);
            parameters.Add(nameof(UserBase.NormalizedUserName), userName.ToLower());
            await _userRepository.UpdateAsync(user, parameters);
        }

        #endregion

        #region IUserEmailStore

        public virtual async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            await _userRepository.UpdateAsync(user);
            return IdentityResult.Success;
        }

        public virtual Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            if (!user.Email.CaseSensitiveEquals(email))
            {
                user.Email = email;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.Email), email);
            return _userRepository.UpdateAsync(user, parameters);
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.EmailConfirmed);
        }

        public virtual async Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            if (user.EmailConfirmed == confirmed)
            {
                user.EmailConfirmed = confirmed;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.EmailConfirmed), confirmed);
            await _userRepository.UpdateAsync(user, parameters);

        }

        public virtual Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            if (normalizedEmail.IsNullOrWhiteSpace())
            {
                return null;
            }
            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserBase.Email), normalizedEmail);
            return _userRepository.QueryFirstOrDefaultAsync(filter);
        }

        public virtual Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.Email);
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(0);
        }

        #endregion

        #region IUserLoginStore

        public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNotNull(login, nameof(login));

            UserLogin ul = new UserLogin
            {
                Id = _idGenerationService.GenerateId(),
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                ProviderKey = login.ProviderKey,
                UserId = user.Id
            };
            return _userLoginRepository.InsertAsync(ul);
        }

        public virtual async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNullOrWhiteSpaceString(loginProvider, nameof(loginProvider));
            Guard.ArgumentNullOrWhiteSpaceString(providerKey, nameof(providerKey));

            SingleQueryFilter query = new SingleQueryFilter();
            query.AddEqual(nameof(UserLogin.UserId), user.Id);
            query.AddEqual(nameof(UserLogin.LoginProvider), loginProvider);
            query.AddEqual(nameof(UserLogin.ProviderKey), providerKey);

            var ul = _userLoginRepository.QueryFirstOrDefault(query);
            if (ul != null)
            {
               await  _userLoginRepository.DeleteAsync(ul);
            }
        }

        public virtual async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));

            SingleQueryFilter query = new SingleQueryFilter();
            query.AddEqual(nameof(UserLogin.UserId), user.Id);

            var ulArray = await _userLoginRepository.QueryAsync(query);
            return ulArray.Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey, ul.ProviderDisplayName)).ToArray();
        }

        public virtual async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNullOrWhiteSpaceString(loginProvider, nameof(loginProvider));
            Guard.ArgumentNullOrWhiteSpaceString(providerKey, nameof(providerKey));

            SingleQueryFilter query = new SingleQueryFilter();
            query.AddEqual(nameof(UserLogin.LoginProvider), loginProvider);
            query.AddEqual(nameof(UserLogin.ProviderKey), providerKey);

            var ul = await _userLoginRepository.QueryFirstOrDefaultAsync(query);

            if (ul != null)
            {
                SingleQueryFilter userFilter = new SingleQueryFilter();
                userFilter.AddEqual(nameof(UserBase.Id), ul.UserId);
                var user = await _userRepository.QueryFirstOrDefaultAsync(userFilter);
                return user;
            }

            return null;

        }

        #endregion

        #region IUserRoleStore

        public virtual async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNullOrWhiteSpaceString(roleName, nameof(roleName));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(RoleBase.Name), roleName);
            var role = await _roleRepository.QueryFirstOrDefaultAsync(filter);
            if (role == null)
            {
                throw new InvalidOperationException($"找不到名称 {roleName} 的角色（Role）。");
            }

            SingleQueryFilter userRoleFilter = new SingleQueryFilter();
            userRoleFilter.AddEqual(nameof(UserRole.UserId), user.Id);
            userRoleFilter.AddEqual(nameof(UserRole.RoleId), role.Id);
            var userRole = await _userRoleRepository.QueryFirstOrDefaultAsync(userRoleFilter);

            if (userRole == null)
            {
                userRole = new UserRole();
                userRole.UserId = user.Id;
                userRole.RoleId = role.Id;
                userRole.RoleName = role.Name;
               await  _userRoleRepository.InsertAsync(userRole);
            }
        }

        public virtual async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNullOrWhiteSpaceString(roleName, nameof(roleName));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(RoleBase.Name), roleName);
            var role = await _roleRepository.QueryFirstOrDefaultAsync(filter);
            if (role == null)
            {
                throw new InvalidOperationException($"找不到名称 {roleName} 的角色（Role）。");
            }

            SingleQueryFilter userRoleFilter = new SingleQueryFilter();
            userRoleFilter.AddEqual(nameof(UserRole.UserId), user.Id);
            userRoleFilter.AddEqual(nameof(UserRole.RoleId), role.Id);
            var userRole = await _userRoleRepository.QueryFirstOrDefaultAsync(userRoleFilter);

            if (userRole != null)
            {
                await _userRoleRepository.DeleteAsync(userRole);
            }
        }

        public virtual async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserRole.UserId), user.Id);
            var userRole = await _userRoleRepository.QueryAsync(filter);
            return userRole.Select(r => r.RoleName).ToList();
        }

        public virtual async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNullOrWhiteSpaceString(roleName, nameof(roleName));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserRole.UserId), user.Id);
            filter.AddEqual(nameof(UserRole.RoleName), roleName);

            var userRole = await _userRoleRepository.QueryFirstOrDefaultAsync(filter);
            return userRole != null;
        }

        public virtual async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNullOrWhiteSpaceString(roleName, nameof(roleName));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserRole.RoleName), roleName);

            var userRoles = await _userRoleRepository.QueryAsync(filter);
            if (userRoles == null)
            {
                return new List<TUser>();
            }

            var userIdArray = userRoles.Select(ur => ur.UserId).ToArray();
            var users = await _userRepository.QueryAsync(fieldName: nameof(UserBase.Id), fieldValues: userIdArray);
            return users.ToList();
        }

        #endregion

        #region IUserClaimStore

        public virtual async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));

            SingleQueryFilter query = new SingleQueryFilter();
            query.AddEqual(nameof(UserClaim.UserId), user.Id);

            var ulArray = await _userClaimRepository.QueryAsync(query);
            return ulArray.Select(ul => ul.ToClaim()).ToArray();

        }

        public virtual async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNotNull(claims, nameof(claims));
            if (claims.Any())
            {
                using (DbTransactionScope scope = new DbTransactionScope())
                {
                    var ucArray = claims.Select(c =>
                    {
                        UserClaim uc = new UserClaim();
                        uc.Id = _idGenerationService.GenerateId();
                        uc.UserId = user.Id;
                        uc.InitializeFromClaim(c);
                        return uc;
                    }).ToArray();

                    foreach (var u in ucArray)
                    {
                        await _userClaimRepository.InsertAsync(u);
                    }
                    scope.Complete();
                }
            }
        }

        public virtual async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNotNull(claim, nameof(claim));
            Guard.ArgumentNotNull(newClaim, nameof(newClaim));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserClaim.UserId), user.Id);
            filter.AddEqual(nameof(UserClaim.ClaimType), claim.Type);

            var userClaim = await _userClaimRepository.QueryFirstOrDefaultAsync(filter);
            if (userClaim != null)
            {
                userClaim.ClaimValue = newClaim.Value;
                await _userClaimRepository.UpdateAsync(userClaim);
            }
        }

        public virtual async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNotNull(claims, nameof(claims));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserClaim.UserId), user.Id);

            var userClaims = await _userClaimRepository.QueryAsync(filter);

            var types = claims.Select(uc => uc.Type).ToArray();
            var toRemove = userClaims.Where(c => types.Contains(c.ClaimType, StringComparer.OrdinalIgnoreCase)).ToArray();

            using (DbTransactionScope scope = new DbTransactionScope())
            {
                foreach (var u in toRemove)
                {
                    await _userClaimRepository.DeleteAsync(u);
                }
                scope.Complete();
            }
        }

        public virtual async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(claim, nameof(claim));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserClaim.ClaimValue), claim.Value);
            filter.AddEqual(nameof(UserClaim.ClaimType), claim.Type);

            var userClaim = await _userClaimRepository.QueryFirstOrDefaultAsync(filter);

            if (userClaim != null)
            {
                SingleQueryFilter userFilter = new SingleQueryFilter();
                userFilter.AddEqual(nameof(UserBase.Id), userClaim.UserId);

                var user = await _userRepository.QueryFirstOrDefaultAsync(userFilter);
                return user == null ? new List<TUser>() : new List<TUser>() { user };
            }
            return new List<TUser>();
        }
        #endregion

        #region IUserPasswordStore

        public virtual Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            if (!user.PasswordHash.CaseSensitiveEquals(passwordHash))
            {
                user.PasswordHash = passwordHash;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.PasswordHash), passwordHash);
            return _userRepository.UpdateAsync(user, parameters);
        }

        public virtual Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!user.PasswordHash.IsNullOrWhiteSpace());
        }

        #endregion

        #region IUserSecurityStampStore

        public virtual Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNullOrWhiteSpaceString(stamp, nameof(stamp));
            if (!user.SecurityStamp.CaseSensitiveEquals(stamp))
            {
                user.SecurityStamp = stamp;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.SecurityStamp), stamp);
            return _userRepository.UpdateAsync(user, parameters);
        }

        public virtual Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.SecurityStamp);
        }

        #endregion

        #region IUserLockoutStore

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user?.LockoutEnd);
        }

        public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            if (user.LockoutEnd != lockoutEnd)
            {
                user.LockoutEnd = lockoutEnd?.UtcDateTime;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.LockoutEnd), lockoutEnd);
            return _userRepository.UpdateAsync(user, parameters);
        }

        public virtual async Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            user.AccessFailedCount += 1;
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.AccessFailedCount), user.AccessFailedCount);
            await _userRepository.UpdateAsync(user, parameters);
            return user.AccessFailedCount;
        }

        public virtual Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.AccessFailedCount), 0);
            return _userRepository.UpdateAsync(user, parameters);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            if (user.LockoutEnabled != enabled)
            {
                user.LockoutEnabled = enabled;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.LockoutEnabled), enabled);
            return _userRepository.UpdateAsync(user, parameters);
        }

        #endregion

        #region IUserPhoneNumberStore

        public virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            if (!user.PhoneNumber.CaseSensitiveEquals(phoneNumber))
            {
                user.PhoneNumber = phoneNumber;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.PhoneNumber), phoneNumber);
            return _userRepository.UpdateAsync(user, parameters);
        }

        public virtual Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.PhoneNumber);
        }

        public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            if (user.PhoneNumberConfirmed != user.PhoneNumberConfirmed)
            {
                user.PhoneNumberConfirmed = confirmed;
            }
            Dictionary<String, Object> parameters = new Dictionary<String, Object>();
            parameters.Add(nameof(UserBase.PhoneNumberConfirmed), confirmed);
            return _userRepository.UpdateAsync(user, parameters);
        }

        #endregion

        #region IUserTwoFactorStore

        public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IUserAuthenticationTokenStore

        public virtual async Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNullOrWhiteSpaceString(loginProvider, nameof(loginProvider));
            Guard.ArgumentNullOrWhiteSpaceString(name, nameof(name));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserToken.UserId), user.Id);
            filter.AddEqual(nameof(UserToken.LoginProvider), loginProvider);
            filter.AddEqual(nameof(UserToken.Name), name);

            var token = await _userTokenRepository.QueryFirstOrDefaultAsync(filter);
            if (token == null)
            {
                UserToken newToken = new UserToken();
                newToken.Id = _idGenerationService.GenerateId();
                newToken.Name = name;
                newToken.Value = value;
                newToken.UserId = user.Id;
                await _userTokenRepository.InsertAsync(newToken);
            }
            else
            {
                token.Value = value;
                await _userTokenRepository.UpdateAsync(token);
            }
        }

        public virtual async Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNullOrWhiteSpaceString(loginProvider, nameof(loginProvider));
            Guard.ArgumentNullOrWhiteSpaceString(name, nameof(name));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserToken.UserId), user.Id);
            filter.AddEqual(nameof(UserToken.LoginProvider), loginProvider);
            filter.AddEqual(nameof(UserToken.Name), name);

            var token = await _userTokenRepository.QueryFirstOrDefaultAsync(filter);
            if (token != null)
            {
                await _userTokenRepository.DeleteAsync(token);
            }
        }

        public virtual async Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNullOrWhiteSpaceString(loginProvider, nameof(loginProvider));
            Guard.ArgumentNullOrWhiteSpaceString(name, nameof(name));

            SingleQueryFilter filter = new SingleQueryFilter();
            filter.AddEqual(nameof(UserToken.UserId), user.Id);
            filter.AddEqual(nameof(UserToken.LoginProvider), loginProvider);
            filter.AddEqual(nameof(UserToken.Name), name);

            var token = await _userTokenRepository.QueryFirstOrDefaultAsync(filter);
            return token?.Value;
        }

        #endregion
    }
}
