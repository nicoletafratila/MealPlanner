using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class EditRecipeModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [MaxLength(512000)]
        public byte[]? ImageContent { get; set; }

        public string? ImageUrl { get; set; }

        public IEnumerable<IngredientModel>? Ingredients { get; set; }
    }
}
