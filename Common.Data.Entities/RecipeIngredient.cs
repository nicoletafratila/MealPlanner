using System.ComponentModel.DataAnnotations.Schema;
using Common.Data.Entities.Converters;

namespace Common.Data.Entities
{
    public class RecipeIngredient
    {
        [ForeignKey(nameof(RecipeId))]
        public Recipe? Recipe { get; set; }
        public int RecipeId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        [ForeignKey(nameof(UnitId))]
        public Unit? Unit { get; set; }
        public int UnitId { get; set; }

        public ShoppingListProduct ToShoppingListProduct(int displaySequence)
        {
            if (Product is null)
                throw new InvalidOperationException("Product must be set before converting to ShoppingListProduct.");

            if (Product.BaseUnit is null)
                throw new InvalidOperationException("Product.BaseUnit must be set before converting to ShoppingListProduct.");

            if (Unit is null)
                throw new InvalidOperationException("Unit must be set before converting to ShoppingListProduct.");

            var convertedQuantity = UnitConverter.Convert(Quantity, Unit, Product.BaseUnit);

            return new ShoppingListProduct
            {
                ProductId = ProductId,
                Product = Product,
                Quantity = convertedQuantity,
                UnitId = Product.BaseUnit.Id,
                Unit = Product.BaseUnit,
                Collected = false,
                DisplaySequence = displaySequence
            };
        }
    }
}