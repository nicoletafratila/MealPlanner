using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Commands.UpdateProductCollected
{
    /// <summary>
    /// Validates UpdateProductCollectedCommand.
    /// </summary>
    public class UpdateProductCollectedCommandValidator : AbstractValidator<UpdateProductCollectedCommand>
    {
        public UpdateProductCollectedCommandValidator()
        {
            RuleFor(x => x.ShoppingListId)
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.ShoppingListMessages.IdGreaterThanZero);

            RuleFor(x => x.ProductId)
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.ShoppingListMessages.ProductIdRequired);
        }
    }
}
