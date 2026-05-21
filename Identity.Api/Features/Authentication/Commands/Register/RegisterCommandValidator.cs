using FluentValidation;
using Identity.Api.Features.Authentication.Resources;

namespace Identity.Api.Features.Authentication.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
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

                    RuleFor(x => x.Model!.EmailAddress)
                        .NotEmpty()
                        .WithMessage(AuthenticationMessages.EmailRequired)
                        .EmailAddress()
                        .WithMessage(AuthenticationMessages.EmailInvalid);
                });
        }
    }
}
