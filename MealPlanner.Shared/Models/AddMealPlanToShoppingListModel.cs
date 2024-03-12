using Common.Shared;

namespace MealPlanner.Shared.Models
{
    public class AddMealPlanToShoppingListModel : BaseModel
    {
        public int MealPlanId { get; set; }
        public int ShoppingListId { get; set; }
    }
}
