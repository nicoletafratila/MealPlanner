using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.UpdateShoppingList
{
    public class UpdateShoppingListCommandValidator : AbstractValidator<UpdateShoppingListCommand>
    {
        public UpdateShoppingListCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
