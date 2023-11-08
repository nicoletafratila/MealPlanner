using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlansByRecipeId
{
    public class SearchMealPlansByRecipeIdQuery : IRequest<IList<MealPlanModel>>
    {
        public int RecipeId { get; set; }
    }
}
