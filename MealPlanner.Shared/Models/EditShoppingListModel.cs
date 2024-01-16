using Common.Shared;
using Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Shared.Models
{
    public class EditShoppingListModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        public int ShopId { get; set; }

        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The recipe requires at least one product.")]
        public IList<ShoppingListProductModel>? Products { get; set; }
    }
}
