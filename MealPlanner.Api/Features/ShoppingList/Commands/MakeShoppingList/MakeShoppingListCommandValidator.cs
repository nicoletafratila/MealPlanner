using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList
{
    public class MakeShoppingListCommandValidator : AbstractValidator<MakeShoppingListCommand>
    {
        public MakeShoppingListCommandValidator()
        {
            RuleFor(x => x.MealPlanId).NotEmpty().GreaterThan(0);
        }
    }
}
