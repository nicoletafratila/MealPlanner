using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Update
{
    /// <summary>
    /// Validates shopping list update commands.
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