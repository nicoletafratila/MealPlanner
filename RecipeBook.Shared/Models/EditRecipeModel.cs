using Common.Shared;
using Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class EditRecipeModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(512000, ErrorMessage = "The image provided is too large.")]
        public byte[]? ImageContent { get; set; }
        public string? ImageUrl { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category for the recipe.")]
        public int RecipeCategoryId { get; set; }

        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The recipe requires at least one ingredient.")]
        public IList<RecipeIngredientModel>? Ingredients { get; set; }
    }
}
