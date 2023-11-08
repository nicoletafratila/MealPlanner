using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Queries.SearchShoppingLists
{
    public class SearchShoppingListsQueryValidator : AbstractValidator<SearchShoppingListsQuery>
    {
        public SearchShoppingListsQueryValidator()
        {
            RuleFor(x => x.QueryParameters).NotNull();
        }
    }
}
