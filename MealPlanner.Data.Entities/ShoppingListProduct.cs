using System.ComponentModel.DataAnnotations.Schema;
using RecipeBook.Data.Entities;

namespace MealPlanner.Data.Entities
{
    public class ShoppingListProduct
    {
        [ForeignKey(nameof(ShoppingListId))]
        public ShoppingList? ShoppingList { get; set; }
        public Guid ShoppingListId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        [ForeignKey(nameof(UnitId))]
        public Unit? Unit { get; set; }
        public Guid UnitId { get; set; }

        public bool Collected { get; set; }
        public int DisplaySequence { get; set; }

        public override string ToString()
            => $"{ProductId} x{Quantity} (Unit {UnitId}) in List {ShoppingListId}";
    }
}
