using FluentValidation;
using Identity.Api.Features.ApplicationUser.Resources;

namespace Identity.Api.Features.ApplicationUser.Commands.ResendConfirmationEmail
{
    public class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
    {
        public ResendConfirmationEmailCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(ApplicationUserMessages.UserIdRequired);
        }
    }
}
