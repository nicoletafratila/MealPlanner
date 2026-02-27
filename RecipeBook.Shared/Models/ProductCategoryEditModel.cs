using System.ComponentModel.DataAnnotations;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Editable model for creating/updating a product category.
    /// </summary>
    public class ProductCategoryEditModel : BaseModel
    {
        /// <summary>
        /// Database identity (0 for new categories).
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Category name (required, max 100 characters).
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public ProductCategoryEditModel()
        {
        }

        public ProductCategoryEditModel(int id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }
}