using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Models;
using Duende.IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    /// <summary>
    /// Handles user login and JWT issuance.
    /// </summary>
    public class LoginCommandHandler(
        UserManager<Common.Data.Entities.ApplicationUser> userManager,
        SignInManager<Common.Data.Entities.ApplicationUser> signInManager,
        ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, CommandResponse?>
    {
        private readonly UserManager<Common.Data.Entities.ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly SignInManager<Common.Data.Entities.ApplicationUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        private readonly ILogger<LoginCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), "Model cannot be null.");

            try
            {
                var username = request.Model.Username ?? string.Empty;
                var password = request.Model.Password ?? string.Empty;

                var user = await _userManager.FindByNameAsync(username);
                if (user is null)
                    return CommandResponse.Failed("Invalid credentials");

                var roles = await _userManager.GetRolesAsync(user);

                var result = await _signInManager.PasswordSignInAsync(
                    username,
                    password,
                    isPersistent: request.Model.RememberLogin,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var claims = GetClaims(user, roles);
                    var token = GenerateJwtToken(claims);

                    return new LoginCommandResponse
                    {
                        Message = "Login successful.",
                        Succeeded = true,
                        JwtBearer = token,
                        Claims = claims
                            .Select(c => new KeyValuePair<string, string>(c.Type, c.Value))
                            .ToList()
                    };
                }

                if (result.IsLockedOut)
                    return CommandResponse.Failed("User is locked out");

                return CommandResponse.Failed("User/password not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred when authenticating the user '{Username}'.",
                    request.Model.Username);
                return CommandResponse.Failed("An error occurred when authenticating the user.");
            }
        }

        private static string GenerateJwtToken(IList<Claim> claims)
        {
            var expiration = DateTimeOffset.UtcNow.AddHours(1);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Common.Constants.MealPlanner.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Common.Constants.MealPlanner.Issuer,
                audience: Common.Constants.MealPlanner.ApiScope,
                claims: claims,
                expires: expiration.UtcDateTime,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static IList<Claim> GetClaims(Common.Data.Entities.ApplicationUser user, IList<string> roles)
        {
            return
            [
                new(JwtRegisteredClaimNames.Sub, user.UserName!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.Role, string.Join(",", roles)),
                new(JwtClaimTypes.Scope, Common.Constants.MealPlanner.ApiScope),
            ];
        }
    }
}