using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetEdit
{
    /// <summary>
    /// Query to retrieve a meal plan for editing.
    /// </summary>
    public class GetEditQuery : IRequest<MealPlanEditModel>
    {
        /// <summary>
        /// Id of the meal plan to edit.
        /// </summary>
        public int Id { get; set; }

        public GetEditQuery()
        {
        }

        public GetEditQuery(int id)
        {
            Id = id;
        }
    }
}