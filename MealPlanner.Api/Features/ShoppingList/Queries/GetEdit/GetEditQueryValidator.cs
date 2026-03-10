using FluentValidation;

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
                .GreaterThan(0)
                .WithMessage("Id must be greater than zero.");
        }
    }
}