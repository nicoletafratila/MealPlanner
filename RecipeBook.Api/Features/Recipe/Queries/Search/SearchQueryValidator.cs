using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Queries.Search
{
    /// <summary>
    /// Validates recipe search queries.
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