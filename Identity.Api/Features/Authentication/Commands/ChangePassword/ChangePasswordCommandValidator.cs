using FluentValidation;
using Identity.Api.Features.Authentication.Resources;

namespace Identity.Api.Features.Authentication.Commands.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(AuthenticationMessages.ModelRequired);

            When(x => x.Model != null, () =>
            {
                RuleFor(x => x.Model!.UserId)
                    .NotEmpty()
                    .WithMessage(AuthenticationMessages.UserIdRequired);

                RuleFor(x => x.Model!.CurrentPassword)
                    .NotEmpty()
                    .WithMessage(AuthenticationMessages.CurrentPasswordRequired);

                RuleFor(x => x.Model!.NewPassword)
                    .NotEmpty()
                    .WithMessage(AuthenticationMessages.NewPasswordRequired);
            });
        }
    }
}
