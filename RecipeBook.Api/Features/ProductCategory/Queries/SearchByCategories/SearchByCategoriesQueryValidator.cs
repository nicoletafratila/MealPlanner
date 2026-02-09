using FluentValidation;

namespace RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories
{
    public class SearchByCategoriesQueryValidator : AbstractValidator<SearchByCategoriesQuery>
    {
        public SearchByCategoriesQueryValidator()
        {
            RuleFor(x => x.CategoryIds).NotNull();
        }
    }
}
