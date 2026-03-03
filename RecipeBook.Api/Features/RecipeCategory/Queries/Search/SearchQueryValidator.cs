using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.Search
{
    /// <summary>
    /// Validates recipe-category search queries.
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