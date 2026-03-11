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
                .GreaterThan(0)
                .WithMessage("RecipeId must be greater than zero.");
        }
    }
}