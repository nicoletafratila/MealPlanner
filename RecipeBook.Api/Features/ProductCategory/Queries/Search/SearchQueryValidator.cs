using FluentValidation;

namespace RecipeBook.Api.Features.ProductCategory.Queries.Search
{
    /// <summary>
    /// Validates product-category search queries.
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