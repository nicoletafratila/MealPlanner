using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Commands.Add
{
    /// <summary>
    /// Validates shop add commands.
    /// </summary>
    public class AddCommandValidator : AbstractValidator<AddCommand>
    {
        public AddCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage("Model is required.");
        }
    }
}