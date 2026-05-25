using FluentValidation;
using Identity.Api.Features.ApplicationUser.Resources;

namespace Identity.Api.Features.ApplicationUser.Commands.Unlock
{
    public class UnlockCommandValidator : AbstractValidator<UnlockCommand>
    {
        public UnlockCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(ApplicationUserMessages.UserIdRequired);
        }
    }
}
