using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CSharpTesterDemoCoreMicrosoftGuide
{
    public class ExternalLoginClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser>
    {
        public ExternalLoginClaimsPrincipalFactory(
            UserManager<IdentityUser> userManager, 
            IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }

        protected async override Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var list = await UserManager.GetClaimsAsync(user);

            ////await UserManager.AddClaimAsync(user, new Claim("foo", "bar"));

            var identity = await base.GenerateClaimsAsync(user);
            return identity;
        }
    }
}
