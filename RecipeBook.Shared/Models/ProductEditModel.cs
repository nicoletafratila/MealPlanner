using System.ComponentModel.DataAnnotations;using Common.Models; using RecipeBook.Shared.Resources;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Editable model for creating/updating a product.
    /// </summary>
    public class ProductEditModel : BaseModel
    {
        /// <summary>
        /// Database identity (0 for new products).
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Product name (required, max 100 characters).
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Raw image content (required, up to 500 KB).
        /// </summary>
        [Required]
        [MaxLength(512000, ErrorMessageResourceName = nameof(RecipeBookSharedMessages.ImageTooLarge), ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
        public byte[]? ImageContent { get; set; }

        /// <summary>
        /// Optional image URL.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Base unit id for this product (e.g., kg, liter).
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessageResourceName = nameof(RecipeBookSharedMessages.ProductUnitRequired), ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
        public int BaseUnitId { get; set; }

        /// <summary>
        /// Product category id.
        /// </summary>
        [Required]
        public Guid ProductCategoryId { get; set; }

        public ProductEditModel()
        {
        }

        public ProductEditModel(int id, string name, int baseUnitId, Guid productCategoryId)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BaseUnitId = baseUnitId;
            ProductCategoryId = productCategoryId;
        }

        public override string ToString() => Name;
    }
}
