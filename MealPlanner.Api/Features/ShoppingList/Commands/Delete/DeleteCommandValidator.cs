using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Delete
{
    public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
