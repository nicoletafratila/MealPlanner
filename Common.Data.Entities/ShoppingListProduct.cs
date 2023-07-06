using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class ShoppingListProduct
    {
        public decimal Quantity { get; set; }
        public bool Collected { get; set; }

        [ForeignKey("ShoppingListId")]
        public ShoppingList? ShoppingList { get; private set; }
        public int ShoppingListId { get; set; }

        [ForeignKey("ProductId")]
        public Ingredient? Product { get; private set; }
        public int ProductId { get; set; }
    }
}
