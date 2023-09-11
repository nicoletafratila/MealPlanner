namespace MealPlanner.Shared.Models
{
    public class ShoppingListModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public IList<ShoppingListProductModel>? Products { get; set; }
    }
}
