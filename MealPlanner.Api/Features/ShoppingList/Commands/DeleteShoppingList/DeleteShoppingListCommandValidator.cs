using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.DeleteShoppingList
{
    public class DeleteShoppingListCommandValidator : AbstractValidator<DeleteShoppingListCommand>
    {
        public DeleteShoppingListCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
