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
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.ShoppingListMessages.MealPlanIdGreaterThanZero);

            RuleFor(x => x.ShopId)
                .GreaterThan(0)
                .WithMessage(Resources.ShoppingListMessages.ShopIdGreaterThanZero);
        }
    }
}