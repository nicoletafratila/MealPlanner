using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId
{
    public class SearchByRecipeIdValidator : AbstractValidator<SearchByRecipeIdQuery>
    {
        public SearchByRecipeIdValidator()
        {
            RuleFor(x => x.RecipeId).NotNull().GreaterThan(0);
        }
    }
}
