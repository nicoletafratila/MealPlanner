using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlansByRecipeId
{
    public class SearchMealPlansByRecipeIdValidator : AbstractValidator<SearchMealPlansByRecipeIdQuery>
    {
        public SearchMealPlansByRecipeIdValidator()
        {
            RuleFor(x => x.RecipeId).NotNull().GreaterThan(0);
        }
    }
}
