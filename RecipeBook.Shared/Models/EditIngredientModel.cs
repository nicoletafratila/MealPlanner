using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class EditIngredientModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the ingredient.")]
        public int UnitId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category for the ingredient.")]
        public int IngredientCategoryId { get; set; }
    }
}
