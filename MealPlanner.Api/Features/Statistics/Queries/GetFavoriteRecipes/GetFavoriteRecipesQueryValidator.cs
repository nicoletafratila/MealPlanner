using FluentValidation;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteRecipes
{
    public class GetFavoriteRecipesQueryValidator : AbstractValidator<GetFavoriteRecipesQuery>
    {
        public GetFavoriteRecipesQueryValidator()
        {
            RuleFor(x => x.Categories).NotEmpty().NotNull();
        }
    }
}
