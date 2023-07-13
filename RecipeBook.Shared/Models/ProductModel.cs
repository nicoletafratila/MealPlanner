namespace RecipeBook.Shared.Models
{
    public class ProductModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public UnitModel? Unit { get; set; }
        public ProductCategoryModel? ProductCategory { get; set; }
    }
}
