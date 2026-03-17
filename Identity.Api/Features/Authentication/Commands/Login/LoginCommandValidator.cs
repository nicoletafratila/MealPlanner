using FluentValidation;

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
                .WithMessage("Model is required.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Model!.Username)
                        .NotEmpty()
                        .WithMessage("Username is required.");

                    RuleFor(x => x.Model!.Password)
                        .NotEmpty()
                        .WithMessage("Password is required.");
                });
        }
    }
}