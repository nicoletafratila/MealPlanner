using FluentValidation;
using Identity.Api.Features.Authentication.Resources;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    /// <summary>
    /// Validates login commands.
    /// </summary>
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(AuthenticationMessages.ModelRequired)
                .DependentRules(() =>
                {
                    RuleFor(x => x.Model!.Username)
                        .NotEmpty()
                        .WithMessage(AuthenticationMessages.UsernameRequired);

                    RuleFor(x => x.Model!.Password)
                        .NotEmpty()
                        .WithMessage(AuthenticationMessages.PasswordRequired);
                });
        }
    }
}