using System.ComponentModel.DataAnnotations;
using Common.Models;
using Common.Validators;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Editable model for creating/updating a shopping list.
    /// </summary>
    public class ShoppingListEditModel : BaseModel
    {
        /// <summary>
        /// Database identity (0 for new lists).
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Shopping list name (required, max 100 characters).
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Linked shop id (required).
        /// </summary>
        [Required]
        public int ShopId { get; set; }

        /// <summary>
        /// List of products in this shopping list.
        /// Must contain at least one item.
        /// </summary>
        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The recipe requires at least one product.")]
        public IList<ShoppingListProductEditModel> Products { get; set; } = new List<ShoppingListProductEditModel>();

        public ShoppingListEditModel()
        {
        }

        public ShoppingListEditModel(int id, string name, int shopId)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ShopId = shopId;
        }

        public override string ToString() => Name;
    }
}