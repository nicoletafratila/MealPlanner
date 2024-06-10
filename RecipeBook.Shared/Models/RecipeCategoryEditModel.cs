using System.ComponentModel.DataAnnotations;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    public class RecipeCategoryEditModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please select the display sequence for the category.")]
        public int DisplaySequence { get; set; }
    }
}
