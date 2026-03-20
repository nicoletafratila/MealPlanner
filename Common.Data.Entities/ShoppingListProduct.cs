using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public sealed class ShoppingListProduct
    {
        [ForeignKey(nameof(ShoppingListId))]
        public ShoppingList? ShoppingList { get; set; }
        public int ShoppingListId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        [ForeignKey(nameof(UnitId))]
        public Unit? Unit { get; set; }
        public int UnitId { get; set; }

        public bool Collected { get; set; }
        public int DisplaySequence { get; set; }

        public override string ToString()
            => $"{ProductId} x{Quantity} (Unit {UnitId}) in List {ShoppingListId}";
    }
}