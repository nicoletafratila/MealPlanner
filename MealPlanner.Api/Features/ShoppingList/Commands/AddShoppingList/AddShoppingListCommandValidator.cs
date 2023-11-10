using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.AddShoppingList
{
    public class AddShoppingListCommandValidator : AbstractValidator<AddShoppingListCommand>
    {
        public AddShoppingListCommandValidator()
        {
            RuleFor(x => x.MealPlanId).NotEmpty().GreaterThan(0);
        }
    }
}
