namespace MealPlanner.Data.Entities
{
    public sealed class Shop : Common.Data.Entities.Entity<Guid>
    {
        public string? UserId { get; set; }

        public string? Name { get; set; }

        public IList<ShopDisplaySequence>? DisplaySequence { get; set; } = [];

        public ShopDisplaySequence? GetDisplaySequence(int? categoryId)
        {
            if (categoryId is null || DisplaySequence is null || DisplaySequence.Count == 0)
                return null;

            return DisplaySequence.FirstOrDefault(i => i.ProductCategoryId == categoryId.Value);
        }

        public override string ToString() => $"{Name} (Id: {Id})";
    }
}
