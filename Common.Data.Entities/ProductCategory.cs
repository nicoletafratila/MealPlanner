namespace Common.Data.Entities
{
    public sealed class ProductCategory : Entity<int>
    {
        public string? Name { get; set; }

        public override string ToString() => $"{Name} (Id: {Id})";
    }
}