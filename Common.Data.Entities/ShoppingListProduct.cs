using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class ShoppingListProduct
    {
        [ForeignKey("ShoppingListId")]
        public ShoppingList? ShoppingList { get; set; }
        public int ShoppingListId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        [ForeignKey("UnitId")]
        public Unit? Unit { get; set; }
        public int? UnitId { get; set; }

        public bool Collected { get; set; }
        public int DisplaySequence { get; set; }
    }
}
