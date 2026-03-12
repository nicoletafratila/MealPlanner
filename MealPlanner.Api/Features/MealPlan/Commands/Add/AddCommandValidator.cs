using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Commands.Add
{
    /// <summary>
    /// Validates meal-plan add commands.
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