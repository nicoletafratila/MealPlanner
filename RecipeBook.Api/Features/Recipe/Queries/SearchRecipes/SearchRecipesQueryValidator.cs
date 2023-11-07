using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Queries.SearchRecipes
{
    public class SearchRecipesQueryValidator : AbstractValidator<SearchRecipesQuery>
    {
        public SearchRecipesQueryValidator()
        {
            RuleFor(x => x.QueryParameters).NotNull();
        }
    }
}
