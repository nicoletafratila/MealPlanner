namespace Common.Data.Entities
{
    public class ShoppingList : Entity<int>
    {
        public string? Name { get; set; }
        public IList<ShoppingListProduct>? Products { get; set; }
    }
}
