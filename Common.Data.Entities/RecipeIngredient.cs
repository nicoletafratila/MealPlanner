using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class RecipeIngredient 
    {
        public decimal Quantity { get; set; }
        
        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }
        public int RecipeId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int ProductId { get; set; }

        public ShoppingListProduct ToShoppingListProduct(int displaySequence)
        {
            var result = new ShoppingListProduct
            {
                ProductId = ProductId,
                Product = Product,
                Quantity = Quantity,
                Collected = false,
                DisplaySequence = displaySequence
            };
            return result;
        }
    }
}
