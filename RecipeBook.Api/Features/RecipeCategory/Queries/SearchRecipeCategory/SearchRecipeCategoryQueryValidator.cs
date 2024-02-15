using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.SearchRecipeCategory
{
    public class SearchRecipeCategoryQueryValidator : AbstractValidator<SearchRecipeCategoryQuery>
    {
        public SearchRecipeCategoryQueryValidator()
        {
            RuleFor(x => x.QueryParameters).NotNull();
        }
    }
}
