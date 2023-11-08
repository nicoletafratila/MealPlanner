using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Queries.GetEditShoppingList
{
    public class GetEditShoppingListQueryValidator : AbstractValidator<GetEditShoppingListQuery>
    {
        public GetEditShoppingListQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
