using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Commands.Update
{
    /// <summary>
    /// Validates shop update commands.
    /// </summary>
    public class UpdateCommandValidator : AbstractValidator<UpdateCommand>
    {
        public UpdateCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage("Model is required.");
        }
    }
}