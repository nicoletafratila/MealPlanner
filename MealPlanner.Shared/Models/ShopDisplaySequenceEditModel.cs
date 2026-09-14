using System.ComponentModel.DataAnnotations;
using Common.Models;
using MealPlanner.Shared.Resources;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Editable display sequence entry for a product category in a shop.
    /// </summary>
    public class ShopDisplaySequenceEditModel : BaseModel
    {
        /// <summary>
        /// Parent shop id. Left as <see cref="Guid.Empty"/> when the shop is being created for the first time;
        /// the server assigns the real shop id and EF Core fixes up this foreign key via the parent shop's
        /// display-sequence navigation collection, so it is not validated as required here.
        /// </summary>
        public Guid ShopId { get; set; }

        /// <summary>
        /// Display index for the product category (0 or greater).
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(MealPlannerSharedMessages.DisplayIndexPositive), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
        public int Value { get; set; }

        /// <summary>
        /// Linked product category.
        /// </summary>
        public ProductCategoryModel? ProductCategory { get; set; }

        public ShopDisplaySequenceEditModel()
        {
        }

        public ShopDisplaySequenceEditModel(Guid shopId, int value, ProductCategoryModel? category)
        {
            ShopId = shopId;
            Value = value;
            ProductCategory = category;
        }

        public override string ToString()
            => ProductCategory?.Name ?? $"Category {Value}";
    }
}
