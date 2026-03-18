using Common.Models;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Lightweight model representing a shopping list.
    /// </summary>
    public class ShoppingListModel : BaseModel
    {
        /// <summary>
        /// Database identity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Shopping list name/title.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public ShoppingListModel()
        {
        }

        public ShoppingListModel(int id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }
}