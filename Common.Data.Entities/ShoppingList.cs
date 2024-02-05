using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class ShoppingList : Entity<int>
    {
        public string? Name { get; set; }

        [ForeignKey("ShopId")]
        public Shop? Shop { get; private set; }
        public int ShopId { get; set; }

        public IList<ShoppingListProduct>? Products { get; set; }
    }
}
