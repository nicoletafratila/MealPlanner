using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class EditMealPlanModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<EditRecipeModel>? Recipes { get; set; }
    }
}
