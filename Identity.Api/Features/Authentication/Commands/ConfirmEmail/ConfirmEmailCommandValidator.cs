using FluentValidation;
using Identity.Api.Features.Authentication.Resources;

namespace Identity.Api.Features.Authentication.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
    {
        public ConfirmEmailCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(AuthenticationMessages.UserIdRequired);

            RuleFor(x => x.Token)
                .NotEmpty();
        }
    }
}
