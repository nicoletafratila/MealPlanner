using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Queries.GetRecipe
{
    public class GetRecipeQueryValidator : AbstractValidator<GetRecipeQuery>
    {
        public GetRecipeQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
