using Common.Shared;

namespace MealPlanner.Shared.Models
{
    public class MakeShoppingListModel : BaseModel
    {
        public int MealPlanId { get; set; }
        public int ShopId { get; set; }
    }
}
