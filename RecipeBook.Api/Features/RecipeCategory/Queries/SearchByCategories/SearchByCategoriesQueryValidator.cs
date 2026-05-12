using FluentValidation;
using RecipeBook.Api.Features.RecipeCategory.Resources;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.SearchByCategories
{
    /// <summary>
    /// Validates SearchByCategoriesQuery for recipe categories.
    /// </summary>
    public class SearchByCategoriesQueryValidator : AbstractValidator<SearchByCategoriesQuery>
    {
        public SearchByCategoriesQueryValidator()
        {
            RuleFor(x => x.CategoryIds)
                .NotNull()
                .WithMessage(RecipeCategoryMessages.CategoryIdsRequired)
                .NotEmpty()
                .WithMessage(RecipeCategoryMessages.CategoryIdsNotEmpty);
        }
    }
}