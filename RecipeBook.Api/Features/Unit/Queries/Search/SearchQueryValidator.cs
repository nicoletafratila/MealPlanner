using FluentValidation;

namespace RecipeBook.Api.Features.Unit.Queries.Search
{
    /// <summary>
    /// Validates search queries for units.
    /// </summary>
    public class SearchQueryValidator : AbstractValidator<SearchQuery>
    {
        public SearchQueryValidator()
        {
            RuleFor(x => x.QueryParameters)
                .NotNull()
                .WithMessage("Query parameters are required.");
        }
    }
}