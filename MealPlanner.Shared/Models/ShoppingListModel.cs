using Common.Shared;

namespace MealPlanner.Shared.Models
{
    public class ShoppingListModel : BaseModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ShopId { get; set; }
        public IList<ShoppingListProductModel>? Products { get; set; }
    }
}
