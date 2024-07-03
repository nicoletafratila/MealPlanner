using System.ComponentModel.DataAnnotations.Schema;
using Common.Data.Entities.Converters;

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

        public decimal Quantity { get; set; }

        [ForeignKey("UnitId")]
        public Unit? Unit { get; set; }
        public int? UnitId { get; set; }

        public ShoppingListProduct ToShoppingListProduct(int displaySequence)
        {
            var result = new ShoppingListProduct
            {
                ProductId = ProductId,
                Product = Product,
                Quantity = UnitConverter.Convert(Quantity, Unit!, Product!.BaseUnit!),
                UnitId = Product!.BaseUnit!.Id,
                Unit = Product!.BaseUnit,
                Collected = false,
                DisplaySequence = displaySequence
            };
            return result;
        }
    }
}
