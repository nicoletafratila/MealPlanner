using Common.Models;

namespace RecipeBook.Shared.Models
{
    public class ProductModel : BaseModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public UnitModel? BaseUnit { get; set; }
        public ProductCategoryModel? ProductCategory { get; set; }
        public string? ProductCategoryName { get; set; }
        public string? ProductCategoryId { get; set; }
    }
}
