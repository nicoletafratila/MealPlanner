using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class Recipe : Entity<int>
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
