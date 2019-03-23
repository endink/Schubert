using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.Domain;

namespace Schubert.Framework.Services
{
    public class NullIdentityService : IIdentityService
    {
        public UserBase CreateAnonymous()
        {
            return null;
        }

        public UserBase GetAuthenticatedUser()
        {
            return null;
        }

        public Task<UserBase> GetByIdAsync(long userId)
        {
            return Task.FromResult<UserBase>(null);
        }

        public Task RefreshIdentityAsync(long userId)
        {
            return Task.FromResult(0);
        }
    }
}
