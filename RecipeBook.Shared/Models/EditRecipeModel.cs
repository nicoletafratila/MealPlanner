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

        //[Required]
        [MaxLength(512000)]
        public byte[] ImageContent { get; set; }
        public string? ImageUrl { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category for the recipe.")]
        public int RecipeCategoryId { get; set; }

        [Required]
        //At leas one element
        public IList<RecipeIngredientModel> Ingredients { get; set; }

        public EditRecipeModel()
        {
            Ingredients = new List<RecipeIngredientModel>();
        }
    }
}
