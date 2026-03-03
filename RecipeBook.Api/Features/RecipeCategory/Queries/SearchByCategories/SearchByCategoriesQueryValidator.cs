using FluentValidation;

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
                .WithMessage("CategoryIds is required.")
                .NotEmpty()
                .WithMessage("CategoryIds cannot be empty.");
        }
    }
}