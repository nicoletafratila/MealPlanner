using Common.Shared.Models;

namespace MealPlanner.Shared.Models
{
    public class ShoppingListCreateModel : BaseModel
    {
        public int MealPlanId { get; set; }
        public int ShopId { get; set; }
    }
}
