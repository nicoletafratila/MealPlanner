using Common.Models;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Lightweight product model used for lists, selections, and display.
    /// </summary>
    public class ProductModel : BaseModel
    {
        /// <summary>
        /// Database identity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Product name. Defaults to empty string to avoid null checks.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional image URL for the product.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Base unit used for this product (e.g., kg, liter).
        /// </summary>
        public UnitModel? BaseUnit { get; set; }

        /// <summary>
        /// Navigation-model for the product category.
        /// </summary>
        public ProductCategoryModel? ProductCategory { get; set; }

        /// <summary>
        /// Cached/flattened product category name.
        /// </summary>
        public string? ProductCategoryName { get; set; }

        /// <summary>
        /// Cached/flattened product category id (as string).
        /// </summary>
        public string? ProductCategoryId { get; set; }

        /// <summary>
        /// Returns the most appropriate category name:
        /// 1) ProductCategory.Name
        /// 2) ProductCategoryName
        /// 3) empty string.
        /// </summary>
        public string EffectiveCategoryName =>
            ProductCategory?.Name
            ?? ProductCategoryName
            ?? string.Empty;

        public ProductModel()
        {
        }

        public ProductModel(int id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }
}