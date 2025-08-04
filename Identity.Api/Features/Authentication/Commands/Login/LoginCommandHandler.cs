using System.Security.Claims;
using Common.Api;
using Common.Data.Entities;
using Common.Models;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, LoginCommandResponse>
    {
        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validate input
                if (request?.Model?.Username == null || request.Model.Password == null)
                {
                    return new LoginCommandResponse
                    {
                        Succeeded = false,
                        Message = "Invalid request.",
                        ErrorCode = "INVALID_INPUT"
                    };
                }

                // 2. Find user
                var user = await userManager.FindByNameAsync(request.Model.Username);
                if (user == null)
                {
                    return new LoginCommandResponse
                    {
                        Succeeded = false,
                        Message = "Invalid credentials.",
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                // 3. Check if user is allowed to log in
                if (!user.EmailConfirmed)
                {
                    return new LoginCommandResponse
                    {
                        Succeeded = false,
                        Message = "Email confirmation required.",
                        ErrorCode = "EMAIL_NOT_CONFIRMED"
                    };
                }

                if (!user.LockoutEnabled || (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow))
                {
                    return new LoginCommandResponse
                    {
                        Succeeded = false,
                        Message = "Account is locked.",
                        ErrorCode = "ACCOUNT_LOCKED"
                    };
                }

                if (!user.IsActive) // Custom property if implementing user disables
                {
                    return new LoginCommandResponse
                    {
                        Succeeded = false,
                        Message = "Account is disabled.",
                        ErrorCode = "ACCOUNT_DISABLED"
                    };
                }

                //var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                //var result = await userManager.ResetPasswordAsync(user, resetToken, request.Model.Password);

                // 4. Try sign-in with password
                var signInResult = await signInManager.PasswordSignInAsync(user, request.Model.Password, isPersistent: false, lockoutOnFailure: true);
                if (signInResult.Succeeded)
                {
                    // 5. Create claims principal
                    var principal = await signInManager.CreateUserPrincipalAsync(user);
                    var identity = (ClaimsIdentity)principal.Identity;
                    var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationTime, unixTime.ToString()));
                    identity.AddClaim(new Claim(JwtClaimTypes.IdentityProvider, "local"));

                    // 6. Create access/refresh token(s) through IdentityServer
                    var validatedResources = new Duende.IdentityServer.Models.Resources(IdentityConfig.IdentityResources, IdentityConfig.ApiResources, IdentityConfig.ApiScopes);
                    var resourceValidationResult = new ResourceValidationResult(validatedResources);
                    var validatedTokenRequest = new ValidatedTokenRequest
                    {
                        Client = IdentityConfig.Clients.FirstOrDefault(),       // Required: the client for which the token is issued
                        ClientId = IdentityConfig.Clients.FirstOrDefault().ClientId,
                        GrantType = "password",  // Or use "client_credentials", "password", etc., as appropriate
                        Subject = principal,    // User context, or null for client credentials flow
                                               // Optionally: set Raw (NameValueCollection), SessionId, Nonce, AccessTokenLifetime, etc.
                    };
                    var tokenRequest = new TokenCreationRequest
                    {
                        Subject = principal,
                        IncludeAllIdentityClaims = true,
                        ValidatedResources = resourceValidationResult,
                        ValidatedRequest = validatedTokenRequest,
                    };


                    var tokenResult = await tokenService.CreateAccessTokenAsync(tokenRequest);
                    var jwt = await tokenService.CreateSecurityTokenAsync(tokenResult);
                    await signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal);

                    return new LoginCommandResponse
                    {
                        Succeeded = true,
                        JwtBearer = jwt,
                        Message = string.Empty,
                        ErrorCode = null
                    };
                }

                // 7. Handle various sign-in errors
                if (signInResult.IsLockedOut)
                {
                    return new LoginCommandResponse
                    {
                        Succeeded = false,
                        Message = "Account is locked due to multiple failed login attempts.",
                        ErrorCode = "ACCOUNT_LOCKED",
                    };
                }
                if (signInResult.IsNotAllowed)
                {
                    return new LoginCommandResponse
                    {
                        Succeeded = false,
                        Message = "Login is not allowed.",
                        ErrorCode = "NOT_ALLOWED"
                    };
                }

                return new LoginCommandResponse
                {
                    Succeeded = false,
                    Message = "Invalid credentials.",
                    ErrorCode = "INVALID_CREDENTIALS"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Login attempt failed for user {Username}", request.Model?.Username ?? "unknown");
                return new LoginCommandResponse
                {
                    Succeeded = false,
                    Message = "An error occurred while processing authentication.",
                    ErrorCode = "SERVER_ERROR"
                };
            }
        }
    }
}
