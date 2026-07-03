using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Delete
{
    /// <summary>
    /// Validates DeleteCommand for shopping lists.
    /// </summary>
    public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.ShoppingListMessages.IdGreaterThanZero);
        }
    }
}