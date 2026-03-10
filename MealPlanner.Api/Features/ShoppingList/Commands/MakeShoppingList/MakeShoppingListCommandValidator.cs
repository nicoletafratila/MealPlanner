using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList
{
    /// <summary>
    /// Validates MakeShoppingListCommand.
    /// </summary>
    public class MakeShoppingListCommandValidator : AbstractValidator<MakeShoppingListCommand>
    {
        public MakeShoppingListCommandValidator()
        {
            RuleFor(x => x.MealPlanId)
                .GreaterThan(0)
                .WithMessage("MealPlanId must be greater than zero.");

            RuleFor(x => x.ShopId)
                .GreaterThan(0)
                .WithMessage("ShopId must be greater than zero.");
        }
    }
}