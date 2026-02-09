using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.SearchByCategories
{
    public class SearchByCategoriesQueryValidator : AbstractValidator<SearchByCategoriesQuery>
    {
        public SearchByCategoriesQueryValidator()
        {
            RuleFor(x => x.CategoryIds).NotNull();
        }
    }
}
