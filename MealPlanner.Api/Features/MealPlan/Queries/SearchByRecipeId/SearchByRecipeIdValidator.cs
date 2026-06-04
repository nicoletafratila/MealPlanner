using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId
{
    /// <summary>
    /// Validates SearchByRecipeIdQuery.
    /// </summary>
    public class SearchByRecipeIdValidator : AbstractValidator<SearchByRecipeIdQuery>
    {
        public SearchByRecipeIdValidator()
        {
            RuleFor(x => x.RecipeId)
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.MealPlanMessages.RecipeIdGreaterThanZero);
        }
    }
}