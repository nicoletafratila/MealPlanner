using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class RecipeIngredient
    {
        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }
        public int RecipeId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int ProductId { get; set; }

        public double Quantity { get; set; }

        [ForeignKey("UnitId")]
        public Unit? Unit { get; private set; }
        public int UnitId { get; set; }

        public ShoppingListProduct ToShoppingListProduct(int displaySequence)
        {
            var result = new ShoppingListProduct
            {
                ProductId = ProductId,
                Product = Product,
                Quantity = Quantity,
                UnitId = UnitId,
                Unit = Unit,
                Collected = false,
                DisplaySequence = displaySequence
            };
            return result;
        }
    }
}
