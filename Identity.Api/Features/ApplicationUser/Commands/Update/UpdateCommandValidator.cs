using FluentValidation;

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
                .WithMessage("Model is required.");
            When(x => x.Model != null, () =>
            {
                RuleFor(x => x.Model!.UserId)
                    .NotEmpty()
                    .WithMessage("UserId is required.");

                RuleFor(x => x.Model!.EmailAddress)
                    .NotEmpty()
                    .WithMessage("Email address is required.");
            });
        }
    }
}