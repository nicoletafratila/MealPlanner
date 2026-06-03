using FluentValidation;
using MealPlanner.Api.Features.ShoppingList.Resources;

namespace MealPlanner.Api.Features.ShoppingList.Queries.GetEdit
{
    /// <summary>
    /// Validates GetEditQuery for shopping lists.
    /// </summary>
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(ShoppingListMessages.IdGreaterThanZero);
        }
    }
}