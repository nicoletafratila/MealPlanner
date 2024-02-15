using FluentValidation;

namespace RecipeBook.Api.Features.ProductCategory.Queries.SearchProductCategory
{
    public class SearchProductCategoryQueryValidator : AbstractValidator<SearchProductCategoryQuery>
    {
        public SearchProductCategoryQueryValidator()
        {
            RuleFor(x => x.QueryParameters).NotNull();
        }
    }
}
