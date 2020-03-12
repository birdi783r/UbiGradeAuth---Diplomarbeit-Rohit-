using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;

namespace CSharpTesterAspNet
{
    public static class GoogleProviderHelper
    {
        public const string ProviderName = "Google";

        // Gets the user name and some other information (the MVC5 template
        // on which this sample is based displays it by default). 
        // This does not include the e-mail address, however.
        public const string GoogleScopeProfile = "profile";

        // Needed to get the e-mail address of the user.
        public const string GoogleScopeEmailAddress = "email";

        public const string UserIdFieldName = ProviderName + "UserId";
        public const string AccessTokenFieldName = ProviderName + "AccessToken";
        public const string TokenIssuedFieldName = ProviderName + "TokenIssuedUtc";
        public const string TokenExpiresInFieldName = ProviderName + "TokenExpiresIn";
        public const string RefreshTokenFieldName = ProviderName + "RefreshToken";
        public const string EmailAddressFieldName = ProviderName + "Email";
        public const string NameFieldName = ProviderName + "Name";

        // See AccountController.ChallengeResult.ExecuteResult on how to request Offline access with Google.
        public const string GoogleAccessType = "access_type";
        public const string GoogleOfflineAccessType = "offline";
        public const string GoogleApprovalPromptType = "approval_prompt";
        public const string GoogleForceApproval = "force";

        public const string GoogleClientId = "482412272553-4qetsfmlhi4drvqfcbj465jb2bl781fu.apps.googleusercontent.com";
        public const string GoogleClientSecret = "kqt3Ho1aKJns4Mtg1lBXlFjo";

        // TODO: review the scopes needed - in particular for the drive API
        public static readonly string[] ClassroomIntegrationScopes = new[] {
                "https://www.googleapis.com/auth/drive",
                "https://www.googleapis.com/auth/classroom.coursework.students.readonly",
                "https://www.googleapis.com/auth/classroom.coursework.me.readonly",
                "https://www.googleapis.com/auth/classroom.course-work.readonly",
                "https://www.googleapis.com/auth/classroom.coursework.students",
                "https://www.googleapis.com/auth/classroom.coursework.me",
                "https://www.googleapis.com/auth/classroom.courses",
                "https://www.googleapis.com/auth/classroom.rosters.readonly",
            };

        internal static async Task MoveClaimsFromExternalIdenityToAuthenticatedUserAsync<TUser>(
            IAuthenticationManager authenticationManager, 
            ApplicationUserManager userManager, 
            string userId) where TUser : IdentityUser
        {
            var claimsIdentity = await authenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
            if (claimsIdentity != null)
            {
                Claim newClaim = null;

                // Refresh token may be missing in case if we didn't request Offline access.
                // If it's there, we'll replace older refresh token with a newer one.
                newClaim = claimsIdentity.FindAll(RefreshTokenFieldName).FirstOrDefault();

                // Retrieve the existing claims for the user and clear old values related to the current provider.
                // Unlike UpdateUserClaimsWithRefreshedToken, we can remove RefreshToken-related values as well since
                // MoveClaimsFromExternalIdenityToAuthenticatedUserAsync can set a completely new OAuth token.
                await ClearAuthenticatedUserExternalIdentityAsync(userManager, userId, clearRefreshToken: newClaim != null);

                // Store the new claims. Start with the refresh token if available.
                if (newClaim != null)
                {
                    await userManager.AddClaimAsync(userId, newClaim);
                }

                newClaim = claimsIdentity.FindAll(UserIdFieldName).First();
                await userManager.AddClaimAsync(userId, newClaim);

                newClaim = claimsIdentity.FindAll(AccessTokenFieldName).First();
                await userManager.AddClaimAsync(userId, newClaim);

                newClaim = claimsIdentity.FindAll(TokenIssuedFieldName).First();
                await userManager.AddClaimAsync(userId, newClaim);

                newClaim = claimsIdentity.FindAll(TokenExpiresInFieldName).First();
                await userManager.AddClaimAsync(userId, newClaim);

                newClaim = claimsIdentity.FindAll(EmailAddressFieldName).First();
                await userManager.AddClaimAsync(userId, newClaim);

                newClaim = claimsIdentity.FindAll(NameFieldName).First();
                await userManager.AddClaimAsync(userId, newClaim);

            }
        }

        // clearRefreshToken must be false in case if we don't want to clear the access token completely but only to renew it,
        // and no new refresh token is available. clearRefreshToken must be true if we remove external login of the given user or
        // if we update the token and we have the new refresh token.
        private static async Task ClearAuthenticatedUserExternalIdentityAsync<TUser>(
            UserManager<TUser, string> userManager, 
            string userId, 
            bool clearRefreshToken)
            where TUser : IdentityUser
        {
            IList<Claim> currentClaims = await userManager.GetClaimsAsync(userId);
            foreach (Claim oldClaim in currentClaims)
            {
                if (oldClaim.Type == UserIdFieldName)
                {
                    await userManager.RemoveClaimAsync(userId, oldClaim);
                }
                else if (oldClaim.Type == AccessTokenFieldName)
                {
                    await userManager.RemoveClaimAsync(userId, oldClaim);
                }
                else if (oldClaim.Type == RefreshTokenFieldName && clearRefreshToken)
                {
                    // If the new request comes with the updated refresh token, we remove the old one.
                    await userManager.RemoveClaimAsync(userId, oldClaim);
                }
                else if (oldClaim.Type == TokenIssuedFieldName)
                {
                    await userManager.RemoveClaimAsync(userId, oldClaim);
                }
                else if (oldClaim.Type == TokenExpiresInFieldName)
                {
                    await userManager.RemoveClaimAsync(userId, oldClaim);
                }
                else if (oldClaim.Type == EmailAddressFieldName)
                {
                    await userManager.RemoveClaimAsync(userId, oldClaim);
                }
                else if (oldClaim.Type == NameFieldName)
                {
                    await userManager.RemoveClaimAsync(userId, oldClaim);
                }
            }
        }

        // Gets whether the specifed user has a refresh token in AspNetUserClaims. Presense of a refresh token indicates that
        // the access token can be renewed. Therefore, the access to the external provider's resources like the user's mailbox
        // will not be limited to 1 hour since the access token has been delivered by the provider.
        internal static bool IsRefreshTokenAvail<TUser>(UserManager<TUser, string> userManager, string uid) 
            where TUser : IdentityUser
        {
            IList<Claim> claimsforUser = userManager.GetClaims(uid);
            return claimsforUser.Any(x => x.Type == RefreshTokenFieldName);
        }

        internal static UserCredential CreateUserCredential(ClaimsIdentity identity)
        {
            var initializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = GoogleClientId,
                    ClientSecret = GoogleClientSecret,
                },
                Scopes = ClassroomIntegrationScopes
            };

            var flow = new GoogleAuthorizationCodeFlow(initializer);
            var userId = identity.FindFirstValue(UserIdFieldName);

            var token = new TokenResponse()
            {
                AccessToken = identity.FindFirstValue(AccessTokenFieldName),
                RefreshToken = identity.FindFirstValue(RefreshTokenFieldName),
                IssuedUtc = DateTime.Parse(identity.FindFirstValue(TokenIssuedFieldName)),
                ExpiresInSeconds = long.Parse(identity.FindFirstValue(TokenExpiresInFieldName)),
            };

            return new UserCredential(flow, userId, token);
        }
    }
}