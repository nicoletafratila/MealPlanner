using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Queries.Search
{
    /// <summary>
    /// Validates shop search queries.
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