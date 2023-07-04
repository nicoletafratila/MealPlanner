namespace MealPlanner.Shared.Models
{
    public class ShoppingIngredientModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public string? Quantity { get; set; }
        public bool Collected { get; set; }
    }
}
