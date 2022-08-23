namespace RecipeBook.Shared.Models
{
    public class IngredientModel
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
    }
}
