namespace RecipeBook.Data.Entities
{
    public class ProductCategory : Common.Data.Entities.Entity<Guid>
    {
        public string? UserId { get; set; }

        public string? Name { get; set; }

        public override string ToString() => $"{Name} (Id: {Id})";
    }
}
