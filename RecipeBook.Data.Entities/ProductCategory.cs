namespace RecipeBook.Data.Entities
{
    public sealed class ProductCategory : Common.Data.Entities.Entity<int>
    {
        public string? UserId { get; set; }

        public string? Name { get; set; }

        public override string ToString() => $"{Name} (Id: {Id})";
    }
}
