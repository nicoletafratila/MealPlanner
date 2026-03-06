using FluentValidation;

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
                .WithMessage("CategoryIds is required.")
                .NotEmpty()
                .WithMessage("CategoryIds cannot be empty.");
        }
    }
}