using FluentValidation;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchRecipes
{
    /// <summary>
    /// Validates favorite-recipes statistics search queries.
    /// </summary>
    public class SearchQueryValidator : AbstractValidator<SearchQuery>
    {
        public SearchQueryValidator()
        {
            // Require CategoryIds to be provided and non-empty
            RuleFor(x => x.CategoryIds)
                .NotEmpty()
                .WithMessage("CategoryIds is required.");
        }
    }
}