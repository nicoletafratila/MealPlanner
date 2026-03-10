using FluentValidation;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchProducts
{
    /// <summary>
    /// Validates favorite-products statistics search queries.
    /// </summary>
    public class SearchQueryValidator : AbstractValidator<SearchQuery>
    {
        public SearchQueryValidator()
        {
            RuleFor(x => x.CategoryIds)
                .NotEmpty()
                .WithMessage("CategoryIds is required.");
        }
    }
}