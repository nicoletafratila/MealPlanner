using Common.Models;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Lightweight model representing a product category.
    /// </summary>
    public sealed class ProductCategoryModel : BaseModel
    {
        /// <summary>
        /// Database identity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Category name (e.g., "Dairy", "Snacks").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public ProductCategoryModel()
        {
        }

        public ProductCategoryModel(int id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }
}