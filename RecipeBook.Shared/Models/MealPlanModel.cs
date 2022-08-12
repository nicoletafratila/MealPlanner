using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class MealPlanModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

       // public ICollection<RecipeModel>? Recipes { get; set; }
    }
}
