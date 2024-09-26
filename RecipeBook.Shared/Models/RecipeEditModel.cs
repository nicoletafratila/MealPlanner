using System.ComponentModel.DataAnnotations;
using Common.Models;
using Common.Validators;

namespace RecipeBook.Shared.Models
{
    public class RecipeEditModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(256)]
        public string? Source { get; set; }

        [Required]
        [MaxLength(512000, ErrorMessage = "The image provided is too large.")]
        public byte[]? ImageContent { get; set; }
        public string? ImageUrl { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category for the recipe.")]
        public int RecipeCategoryId { get; set; }

        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The recipe requires at least one ingredient.")]
        public IList<RecipeIngredientEditModel>? Ingredients { get; set; }
    }
}
