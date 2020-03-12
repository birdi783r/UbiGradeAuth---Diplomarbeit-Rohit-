using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using CSharpTesterAspNet.Models;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Serializer;
using Microsoft.Owin.Security.DataHandler.Encoder;
using System.Security.Claims;

namespace CSharpTesterAspNet
{
    public partial class Startup
    {
        #region UnsecureTokenFormatter
        // This class is for debugging purposes only, it's not used unless you set Startup.EncryptCookies to false.
        // To debug cookie authentication, you may disable cookie encryption and set breakpoints in Protect/Unprotect methods
        // to monitor how and when the identity claims are updated in the cookies. Do NOT use unencrypted cookies in production!
        private class UnsecureTokenFormatter : ISecureDataFormat<AuthenticationTicket>
        {
            private IDataSerializer<AuthenticationTicket> serializer;
            private ITextEncoder encoder;

            public UnsecureTokenFormatter()
            {
                serializer = DataSerializers.Ticket;
                encoder = TextEncodings.Base64Url;
            }

            public string Protect(AuthenticationTicket ticket)
            {
                string text = encoder.Encode(serializer.Serialize(ticket));
                return text;
            }

            public AuthenticationTicket Unprotect(string text)
            {
                AuthenticationTicket ticket = serializer.Deserialize(encoder.Decode(text));
                return ticket;
            }
        }
        #endregion

        // Set to false if you don't need a refresh token. Without a refresh token your application will be able
        // to access the user's mailbox for no longer than an hour and you'll get authentication errors afterwards.
        public const bool RequireOfflineAccess = true;

        // If true, default cookie protection mechanism is used (secure). If false, UnsecureTokenFormatter is enabled which
        // can be useful for debugging and understanding when and how authentication cookies are set.
        // You must clear the cookies for the application's web site when you change this value,
        // or you may get NullReferenceException somewhere down the OWIN pipeline otherwise!
        public const bool EncryptCookies = true;

        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                TicketDataFormat = EncryptCookies ? null : new UnsecureTokenFormatter(),
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            var options = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = GoogleProviderHelper.GoogleClientId,
                ClientSecret = GoogleProviderHelper.GoogleClientSecret,
                Provider = new GoogleOAuth2AuthenticationProvider()
                {
                    OnReturnEndpoint = context =>
                    {
                        context.Properties.AllowRefresh = true;
                        context.Properties.Dictionary.Add(GoogleProviderHelper.GoogleAccessType, GoogleProviderHelper.GoogleOfflineAccessType);

                        return Task.FromResult(0);
                    },
                    OnAuthenticated = context =>
                    {
                        context.Identity.AddClaim(new Claim(GoogleProviderHelper.UserIdFieldName, context.Id));

                        // Any exception here will result in 'loginInfo == null' in AccountController.ExternalLoginCallback.
                        // Be sure to add exception handling here in case of production code.
                        context.Identity.AddClaim(new Claim(GoogleProviderHelper.AccessTokenFieldName, context.AccessToken));

                        // For clarity, we don't check most values for null but RefreshToken is another kind of thing. It's usually
                        // not set unless we specially request it. Typically, you receive the refresh token only on the initial request,
                        // store it permanently and reuse it when you need to refresh the access token.
                        if (context.RefreshToken != null)
                        {
                            context.Identity.AddClaim(new Claim(GoogleProviderHelper.RefreshTokenFieldName, context.RefreshToken));
                        }

                        // We want to use the e-mail account of the external identity (for which we doing OAuth). For that we save
                        // the external identity's e-mail address separately as it can be different from the main e-mail address
                        // of the current user. 
                        context.Identity.AddClaim(new Claim(GoogleProviderHelper.EmailAddressFieldName, context.Email));
                        context.Identity.AddClaim(new Claim(GoogleProviderHelper.NameFieldName, context.Name));

                        context.Identity.AddClaim(new Claim(GoogleProviderHelper.TokenIssuedFieldName, DateTime.UtcNow.ToString()));
                        context.Identity.AddClaim(new Claim(GoogleProviderHelper.TokenExpiresInFieldName,
                            ((long)context.ExpiresIn.Value.TotalSeconds).ToString()));

                        return Task.FromResult(0);
                    }
                },
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ExternalCookie
            };

            // The scopes needed for classroom integration
            foreach (var s in GoogleProviderHelper.ClassroomIntegrationScopes)
            {
                options.Scope.Add(s);
            }

            options.Scope.Add(GoogleProviderHelper.GoogleScopeProfile);
            options.Scope.Add(GoogleProviderHelper.GoogleScopeEmailAddress);

            options.AccessType = "offline";

            // See AccountController.ChallengeResult.ExecuteResult on how to request Offline access with Google.

            app.UseGoogleAuthentication(options);
        }
    }
}