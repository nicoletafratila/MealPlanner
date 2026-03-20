using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public sealed class ShoppingList : Entity<int>
    {
        public string? Name { get; set; }

        [ForeignKey(nameof(ShopId))]
        public Shop? Shop { get; set; }
        public int ShopId { get; set; }

        public IList<ShoppingListProduct> Products { get; set; } = [];

        public override string ToString() => $"{Name} (Id: {Id}, ShopId: {ShopId})";
    }
}