using System.ComponentModel.DataAnnotations;
using Common.Models;
using RecipeBook.Shared.Resources;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Editable model for creating/updating a product category.
    /// </summary>
    public class ProductCategoryEditModel : BaseModel
    {
        /// <summary>
        /// Database identity (empty for new categories).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Category name (required, max 100 characters).
        /// </summary>
        [Required(ErrorMessageResourceName = nameof(RecipeBookSharedMessages.ProductCategoryNameRequired), ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public ProductCategoryEditModel()
        {
        }

        public ProductCategoryEditModel(Guid id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }
}