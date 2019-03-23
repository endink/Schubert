using Schubert.Framework.Domain;
using Schubert.Framework.Environment;
using Schubert.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment
{
    public class UserStateProvider : IWorkContextStateProvider
    {
        private IIdentityService _identityService;

        public UserStateProvider(IIdentityService identityService)
        {
            Guard.ArgumentNotNull(identityService, nameof(identityService));
            
            _identityService = identityService;
        }

        public Func<WorkContext, Object> Get(string name)
        {
            if (name == WorkContext.CurrentUserStateName)
            {
                return ctx =>
                {
                    UserBase u = _identityService.GetAuthenticatedUser();
                    return u ?? _identityService.CreateAnonymous();
                };
            }
            return null;
        }
    }
}
