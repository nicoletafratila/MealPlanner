using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.Search
{
    /// <summary>
    /// Validates meal-plan search queries.
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