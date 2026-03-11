using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Add
{
    /// <summary>
    /// Validates shopping list add commands.
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