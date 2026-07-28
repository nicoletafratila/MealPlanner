using FluentValidation;
using Identity.Api.Features.Authentication.Resources;
using Identity.Shared.Resources;

namespace Identity.Api.Features.Authentication.Commands.RefreshToken
{
    /// <summary>
    /// Validates refresh token commands.
    /// </summary>
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(AuthenticationMessages.ModelRequired)
                .DependentRules(() =>
                {
                    RuleFor(x => x.Model!.RefreshToken)
                        .NotEmpty()
                        .WithMessage(IdentitySharedMessages.RefreshTokenRequired);
                });
        }
    }
}
