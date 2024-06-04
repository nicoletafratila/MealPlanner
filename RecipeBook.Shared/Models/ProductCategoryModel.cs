using Common.Shared.Models;

namespace RecipeBook.Shared.Models
{
    public class ProductCategoryModel : BaseModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
