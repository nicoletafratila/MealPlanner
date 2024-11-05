using Common.Models;

namespace RecipeBook.Shared.Models
{
    public class RecipeModel : BaseModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
        public RecipeCategoryModel? RecipeCategory { get; set; }
        public string? RecipeCategoryName { get; set; }
        public string? RecipeCategoryId { get; set; }
    }
}
