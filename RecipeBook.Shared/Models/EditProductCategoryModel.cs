using System.ComponentModel.DataAnnotations;
using Common.Shared;

namespace RecipeBook.Shared.Models
{
    public class EditProductCategoryModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
    }
}
