namespace Common.Data.Entities
{
    public class ProductCategory : Entity<int>
    {
        public string? Name { get; set; }
        public int DisplaySequence { get; set; }
    }
}
