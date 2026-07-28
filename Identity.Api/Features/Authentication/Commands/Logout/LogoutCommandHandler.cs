using Common.Data.DataContext;
using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Features.Authentication.Commands.Logout
{
    /// <summary>
    /// Handles logging out the current user.
    /// </summary>
    public class LogoutCommandHandler(
        SignInManager<Data.Entities.ApplicationUser> signInManager,
        MealPlannerDbContext dbContext) : IRequestHandler<LogoutCommand, CommandResponse?>
    {
        private readonly SignInManager<Data.Entities.ApplicationUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        private readonly MealPlannerDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<CommandResponse?> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                var tokenHash = RefreshTokenGenerator.Hash(request.RefreshToken);
                var existingToken = await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

                if (existingToken is not null && existingToken.RevokedAtUtc is null)
                {
                    existingToken.RevokedAtUtc = DateTime.UtcNow;
                    _dbContext.RefreshTokens.Update(existingToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            await _signInManager.SignOutAsync();
            return CommandResponse.Success();
        }
    }
}