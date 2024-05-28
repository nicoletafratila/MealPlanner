using System.ComponentModel.DataAnnotations;
using Common.Shared;

namespace RecipeBook.Shared.Models
{
    public class EditRecipeCategoryModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
    }
}
