using FluentValidation;
using Identity.Api.Features.ApplicationUser.Resources;

namespace Identity.Api.Features.ApplicationUser.Commands.Update
{
    /// <summary>
    /// Validates user update commands.
    /// </summary>
    public class UpdateCommandValidator : AbstractValidator<UpdateCommand>
    {
        public UpdateCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(ApplicationUserMessages.ModelRequired);
            When(x => x.Model != null, () =>
            {
                RuleFor(x => x.Model!.UserId)
                    .NotEmpty()
                    .WithMessage(ApplicationUserMessages.UserIdRequired);

                RuleFor(x => x.Model!.EmailAddress)
                    .NotEmpty()
                    .WithMessage(ApplicationUserMessages.EmailAddressRequired);
            });
        }
    }
}