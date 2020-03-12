using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CSharpTesterDemoCoreMicrosoftGuide.Areas.Identity
{
    public class GoogleProviderHelper
    {
        ////internal static async Task MoveClaimsFromExternalIdenityToAuthenticatedUserAsync<TUser>(
        ////   IAuthenticationManager authenticationManager,
        ////   ApplicationUserManager userManager,
        ////   string userId) where TUser : IdentityUser
        ////{
        ////    var claimsIdentity = await authenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
        ////    if (claimsIdentity != null)
        ////    {
        ////        Claim newClaim = null;

        ////        // Refresh token may be missing in case if we didn't request Offline access.
        ////        // If it's there, we'll replace older refresh token with a newer one.
        ////        newClaim = claimsIdentity.FindAll(RefreshTokenFieldName).FirstOrDefault();

        ////        // Retrieve the existing claims for the user and clear old values related to the current provider.
        ////        // Unlike UpdateUserClaimsWithRefreshedToken, we can remove RefreshToken-related values as well since
        ////        // MoveClaimsFromExternalIdenityToAuthenticatedUserAsync can set a completely new OAuth token.
        ////        await ClearAuthenticatedUserExternalIdentityAsync(userManager, userId, clearRefreshToken: newClaim != null);

        ////        // Store the new claims. Start with the refresh token if available.
        ////        if (newClaim != null)
        ////        {
        ////            await userManager.AddClaimAsync(userId, newClaim);
        ////        }

        ////        newClaim = claimsIdentity.FindAll(UserIdFieldName).First();
        ////        await userManager.AddClaimAsync(userId, newClaim);

        ////        newClaim = claimsIdentity.FindAll(AccessTokenFieldName).First();
        ////        await userManager.AddClaimAsync(userId, newClaim);

        ////        newClaim = claimsIdentity.FindAll(TokenIssuedFieldName).First();
        ////        await userManager.AddClaimAsync(userId, newClaim);

        ////        newClaim = claimsIdentity.FindAll(TokenExpiresInFieldName).First();
        ////        await userManager.AddClaimAsync(userId, newClaim);

        ////        newClaim = claimsIdentity.FindAll(EmailAddressFieldName).First();
        ////        await userManager.AddClaimAsync(userId, newClaim);

        ////        newClaim = claimsIdentity.FindAll(NameFieldName).First();
        ////        await userManager.AddClaimAsync(userId, newClaim);

        ////    }
        ////}

        ////// clearRefreshToken must be false in case if we don't want to clear the access token completely but only to renew it,
        ////// and no new refresh token is available. clearRefreshToken must be true if we remove external login of the given user or
        ////// if we update the token and we have the new refresh token.
        ////private static async Task ClearAuthenticatedUserExternalIdentityAsync<TUser>(
        ////    UserManager<TUser, string> userManager,
        ////    string userId,
        ////    bool clearRefreshToken)
        ////    where TUser : IdentityUser
        ////{
        ////    IList<Claim> currentClaims = await userManager.GetClaimsAsync(userId);
        ////    foreach (Claim oldClaim in currentClaims)
        ////    {
        ////        if (oldClaim.Type == UserIdFieldName)
        ////        {
        ////            await userManager.RemoveClaimAsync(userId, oldClaim);
        ////        }
        ////        else if (oldClaim.Type == AccessTokenFieldName)
        ////        {
        ////            await userManager.RemoveClaimAsync(userId, oldClaim);
        ////        }
        ////        else if (oldClaim.Type == RefreshTokenFieldName && clearRefreshToken)
        ////        {
        ////            // If the new request comes with the updated refresh token, we remove the old one.
        ////            await userManager.RemoveClaimAsync(userId, oldClaim);
        ////        }
        ////        else if (oldClaim.Type == TokenIssuedFieldName)
        ////        {
        ////            await userManager.RemoveClaimAsync(userId, oldClaim);
        ////        }
        ////        else if (oldClaim.Type == TokenExpiresInFieldName)
        ////        {
        ////            await userManager.RemoveClaimAsync(userId, oldClaim);
        ////        }
        ////        else if (oldClaim.Type == EmailAddressFieldName)
        ////        {
        ////            await userManager.RemoveClaimAsync(userId, oldClaim);
        ////        }
        ////        else if (oldClaim.Type == NameFieldName)
        ////        {
        ////            await userManager.RemoveClaimAsync(userId, oldClaim);
        ////        }
        ////    }
        ////}
    }
}
