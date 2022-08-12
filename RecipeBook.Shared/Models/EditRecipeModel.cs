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

        public byte[]? ImageContent { get; set; }

        public string? ImageUrl { get; set; }

        public ICollection<IngredientModel>? Ingredients { get; set; }
    }
}
