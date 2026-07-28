using Common.Data.DataContext;
using Common.Models;
using Identity.Api.Features.Authentication.Resources;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Features.Authentication.Commands.RefreshToken
{
    /// <summary>
    /// Exchanges a valid refresh token for a new JWT, rotating the refresh token.
    /// </summary>
    public class RefreshTokenCommandHandler(
        UserManager<Data.Entities.ApplicationUser> userManager,
        MealPlannerDbContext dbContext,
        ILogger<RefreshTokenCommandHandler> logger) : IRequestHandler<RefreshTokenCommand, CommandResponse?>
    {
        private readonly UserManager<Data.Entities.ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly MealPlannerDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly ILogger<RefreshTokenCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), AuthenticationMessages.ModelCannotBeNull);

            try
            {
                var tokenHash = RefreshTokenGenerator.Hash(request.Model.RefreshToken);
                var existingToken = await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

                if (existingToken is null || existingToken.RevokedAtUtc is not null)
                    return CommandResponse.Failed(AuthenticationMessages.InvalidRefreshToken);

                if (existingToken.ExpiresAtUtc <= DateTime.UtcNow)
                    return CommandResponse.Failed(AuthenticationMessages.RefreshTokenExpired);

                var user = await _userManager.FindByIdAsync(existingToken.UserId);
                if (user is null || !user.IsActive)
                    return CommandResponse.Failed(AuthenticationMessages.InvalidRefreshToken);

                var (newEntity, newRawToken) = RefreshTokenGenerator.CreateEntity(user.Id, RefreshTokenGenerator.DefaultLifetime);

                existingToken.RevokedAtUtc = DateTime.UtcNow;
                existingToken.ReplacedByTokenId = newEntity.Id;
                _dbContext.RefreshTokens.Update(existingToken);
                await _dbContext.RefreshTokens.AddAsync(newEntity, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var roles = await _userManager.GetRolesAsync(user);
                var claims = JwtTokenFactory.GetClaims(user, roles);
                var jwt = JwtTokenFactory.GenerateJwtToken(claims);

                return new LoginCommandResponse
                {
                    Message = AuthenticationMessages.LoginSuccessful,
                    Succeeded = true,
                    JwtBearer = jwt,
                    RefreshToken = newRawToken,
                    Claims = claims
                        .Select(c => new KeyValuePair<string, string>(c.Type, c.Value))
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing a token.");
                return CommandResponse.Failed(AuthenticationMessages.AuthenticationError);
            }
        }
    }
}
