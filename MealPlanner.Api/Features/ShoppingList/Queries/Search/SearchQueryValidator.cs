using FluentValidation;

namespace MealPlanner.Api.Features.ShoppingList.Queries.Search
{
    /// <summary>
    /// Validates shopping-list search queries.
    /// </summary>
    public class SearchQueryValidator : AbstractValidator<SearchQuery>
    {
        public SearchQueryValidator()
        {
            RuleFor(x => x.QueryParameters)
                .NotNull()
                .WithMessage("QueryParameters is required.");
        }
    }
}