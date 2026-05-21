using FluentValidation;
using Identity.Api.Features.Authentication.Resources;

namespace Identity.Api.Features.Authentication.Commands.ForgotPassword
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(AuthenticationMessages.ModelRequired)
                .DependentRules(() =>
                {
                    RuleFor(x => x.Model!.EmailAddress)
                        .NotEmpty()
                        .WithMessage(AuthenticationMessages.EmailRequired)
                        .EmailAddress()
                        .WithMessage(AuthenticationMessages.EmailInvalid);
                });
        }
    }
}
