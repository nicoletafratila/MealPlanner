using System.ComponentModel.DataAnnotations;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    public class ProductEditModel : BaseModel
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
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the product.")]
        public int BaseUnitId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category for the product.")]
        public int ProductCategoryId { get; set; }
    }
}
