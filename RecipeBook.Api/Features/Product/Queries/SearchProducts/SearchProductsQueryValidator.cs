using FluentValidation;

namespace RecipeBook.Api.Features.Product.Queries.SearchProducts
{
    public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
    {
        public SearchProductsQueryValidator()
        {
            RuleFor(x => x.QueryParameters).NotNull();
        }
    }
}
