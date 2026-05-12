using FluentValidation;
using RecipeBook.Api.Features.ProductCategory.Resources;

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
                .WithMessage(ProductCategoryMessages.QueryParametersRequired);
        }
    }
}