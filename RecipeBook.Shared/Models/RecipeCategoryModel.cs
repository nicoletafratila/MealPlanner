using Common.Models;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Lightweight model representing a recipe category.
    /// </summary>
    public class RecipeCategoryModel : BaseModel
    {
        /// <summary>
        /// Database identity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Category name (e.g., "Desert", "Fel principal").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Display order in lists/menus.
        /// </summary>
        public int DisplaySequence { get; set; }

        public RecipeCategoryModel()
        {
        }

        public RecipeCategoryModel(int id, string name, int displaySequence = 0)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplaySequence = displaySequence;
        }

        public override string ToString() => Name;
    }
}