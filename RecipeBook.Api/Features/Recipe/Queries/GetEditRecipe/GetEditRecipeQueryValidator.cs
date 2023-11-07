using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEditRecipe
{
    public class GetEditRecipeQueryValidator : AbstractValidator<GetEditRecipeQuery>
    {
        public GetEditRecipeQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
