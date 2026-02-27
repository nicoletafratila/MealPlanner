using System.ComponentModel.DataAnnotations;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Editable model for creating/updating a recipe category.
    /// </summary>
    public class RecipeCategoryEditModel : BaseModel
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

        /// <summary>
        /// Display order in lists/menus. 0 or greater.
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please select the display sequence for the category.")]
        public int DisplaySequence { get; set; }

        public RecipeCategoryEditModel()
        {
        }

        public RecipeCategoryEditModel(int id, string name, int displaySequence)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplaySequence = displaySequence;
        }

        public override string ToString() => Name;
    }
}