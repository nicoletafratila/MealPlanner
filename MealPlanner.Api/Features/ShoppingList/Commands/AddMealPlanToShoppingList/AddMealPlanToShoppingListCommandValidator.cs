using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.AddMealPlanToShoppingList
{
    public class AddMealPlanToShoppingListCommandValidator : AbstractValidator<AddMealPlanToShoppingListCommand>
    {
        public AddMealPlanToShoppingListCommandValidator()
        {
            RuleFor(x => x.MealPlanId).NotEmpty().GreaterThan(0);
            RuleFor(x => x.ShoppingListId).NotEmpty().GreaterThan(0);
        }
    }
}
