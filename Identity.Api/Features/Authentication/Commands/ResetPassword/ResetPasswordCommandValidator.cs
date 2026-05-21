using FluentValidation;
using Identity.Api.Features.Authentication.Resources;

namespace Identity.Api.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(AuthenticationMessages.ModelRequired)
                .DependentRules(() =>
                {
                    RuleFor(x => x.Model!.UserId)
                        .NotEmpty();

                    RuleFor(x => x.Model!.Token)
                        .NotEmpty();

                    RuleFor(x => x.Model!.NewPassword)
                        .NotEmpty()
                        .WithMessage(AuthenticationMessages.NewPasswordRequired);

                    RuleFor(x => x.Model!.ConfirmPassword)
                        .NotEmpty()
                        .Equal(x => x.Model!.NewPassword)
                        .WithMessage(AuthenticationMessages.PasswordsDoNotMatch);
                });
        }
    }
}
