using Common.Shared.Models;

namespace MealPlanner.Shared.Models
{
    public class MealPlanModel : BaseModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
