using System.ComponentModel.DataAnnotations;
using Common.Models;
using Common.Validators;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Editable model for creating/updating a recipe.
    /// </summary>
    public class RecipeEditModel : BaseModel
    {
        /// <summary>
        /// Database identity (0 for new recipes).
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Recipe name (required, max 100 chars).
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional source (e.g., URL, cookbook, author).
        /// </summary>
        [StringLength(256)]
        public string? Source { get; set; }

        /// <summary>
        /// Raw image content (required, up to 500 KB).
        /// </summary>
        [Required]
        [MaxLength(512000, ErrorMessage = "The image provided is too large.")]
        public byte[]? ImageContent { get; set; }

        /// <summary>
        /// Optional image URL (when not using ImageContent).
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Selected recipe category id.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category for the recipe.")]
        public int RecipeCategoryId { get; set; }

        /// <summary>
        /// Ingredient lines; must contain at least one item.
        /// </summary>
        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The recipe requires at least one ingredient.")]
        public IList<RecipeIngredientEditModel> Ingredients { get; set; } = new List<RecipeIngredientEditModel>();

        public RecipeEditModel()
        {
        }

        public RecipeEditModel(int id, string name, int recipeCategoryId)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            RecipeCategoryId = recipeCategoryId;
        }

        public override string ToString() => Name;
    }
}