using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId
{
    /// <summary>
    /// Query to retrieve meal plans that contain a given recipe.
    /// </summary>
    public class SearchByRecipeIdQuery : IRequest<IList<MealPlanModel>>
    {
        /// <summary>
        /// Id of the recipe to search meal plans for.
        /// </summary>
        public int RecipeId { get; set; }

        public SearchByRecipeIdQuery()
        {
        }

        public SearchByRecipeIdQuery(int recipeId)
        {
            RecipeId = recipeId;
        }
    }
}