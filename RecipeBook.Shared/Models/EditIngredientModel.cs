using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class EditIngredientModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required] 
        public string Unit { get; set; }
        [Required]
        public int IngredientCategoryId { get; set; }
    }
}
