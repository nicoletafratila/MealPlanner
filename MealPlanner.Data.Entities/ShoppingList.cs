using System.ComponentModel.DataAnnotations.Schema;

namespace MealPlanner.Data.Entities
{
    public sealed class ShoppingList : Common.Data.Entities.Entity<int>
    {
        public string? UserId { get; set; }

        public string? Name { get; set; }

        [ForeignKey(nameof(ShopId))]
        public Shop? Shop { get; set; }
        public Guid ShopId { get; set; }

        public IList<ShoppingListProduct>? Products { get; set; } = [];

        public override string ToString() => $"{Name} (Id: {Id}, ShopId: {ShopId})";
    }
}
