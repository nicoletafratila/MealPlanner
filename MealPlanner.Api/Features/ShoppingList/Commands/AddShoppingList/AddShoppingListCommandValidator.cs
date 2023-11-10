using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.AddShoppingList
{
    public class AddShoppingListCommandValidator : AbstractValidator<AddShoppingListCommand>
    {
        public AddShoppingListCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
