using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeBook.Data.Entities
{
    public class Recipe : Common.Data.Entities.Entity<int>
    {
        public string? UserId { get; set; }

        public string? Name { get; set; }

        public byte[]? ImageContent { get; set; }

        public string? Source { get; set; }

        [ForeignKey("RecipeCategoryId")]
        public RecipeCategory? RecipeCategory { get; set; }
        public int RecipeCategoryId { get; set; }

        public IList<RecipeIngredient>? RecipeIngredients { get; set; }
    }
}
