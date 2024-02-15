namespace Common.Data.Entities
{
    public class Shop : Entity<int>
    {
        public string? Name { get; set; }
        public IList<ShopDisplaySequence>? DisplaySequence { get; set; }

        public ShopDisplaySequence GetDisplaySequence(int? categoryId)
        {
            return DisplaySequence?.FirstOrDefault(i => i.ProductCategoryId == categoryId)!;
        }
    }
}
