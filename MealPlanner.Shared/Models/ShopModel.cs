using Common.Models;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Lightweight model representing a shop.
    /// </summary>
    public sealed class ShopModel : BaseModel
    {
        /// <summary>
        /// Database identity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Shop name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public ShopModel()
        {
        }

        public ShopModel(int id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }
}