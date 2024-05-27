using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId
{
    public class SearchByRecipeIdQuery : IRequest<IList<MealPlanModel>>
    {
        public int RecipeId { get; set; }
    }
}
