namespace RecipeBook.Shared.Models
{
    public class RecipeIngredientModel
    {
        public int RecipeId { get; set; }
        public IngredientModel Ingredient { get; set; }
        public decimal Quantity { get; set; }

        public RecipeIngredientModel()
        {
            Ingredient= new IngredientModel();
        }
    }
}
