using FluentValidation;
using RecipeBook.Api.Features.ProductCategory.Resources;

namespace RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories
{
    /// <summary>
    /// Validates SearchByCategoriesQuery for product categories.
    /// </summary>
    public class SearchByCategoriesQueryValidator : AbstractValidator<SearchByCategoriesQuery>
    {
        public SearchByCategoriesQueryValidator()
        {
            RuleFor(x => x.CategoryIds)
                .NotNull()
                .WithMessage(ProductCategoryMessages.CategoryIdsRequired)
                .NotEmpty()
                .WithMessage(ProductCategoryMessages.CategoryIdsNotEmpty);
        }
    }
}