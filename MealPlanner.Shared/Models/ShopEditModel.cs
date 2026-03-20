using System.ComponentModel.DataAnnotations;
using Common.Models;
using Common.Validators;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Editable model for creating/updating a shop, including product category display order.
    /// </summary>
    public class ShopEditModel : BaseModel
    {
        /// <summary>
        /// Database identity (0 for new shops).
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Shop name (required, max 100 characters).
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product category display sequence for this shop.
        /// Must contain at least one entry.
        /// </summary>
        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The shop requires at least product category order.")]
        public IList<ShopDisplaySequenceEditModel>? DisplaySequence { get; set; } = [];

        public ShopEditModel()
        {
        }

        /// <summary>
        /// Creates a shop edit model with display sequence initialized from given categories.
        /// </summary>
        public ShopEditModel(IList<ProductCategoryModel>? categories)
        {
            DisplaySequence = [];

            if (categories is null)
                return;

            var index = 1;
            foreach (var category in categories)
            {
                if (category is null)
                    continue;

                DisplaySequence.Add(new ShopDisplaySequenceEditModel
                {
                    Index = index,
                    ShopId = Id, // at this point Id is whatever the caller has set (often 0 for new shops)
                    ProductCategory = category,
                    Value = index
                });

                index++;
            }
        }

        /// <summary>
        /// Returns the display sequence entry for a given category id, or null if not found.
        /// </summary>
        public ShopDisplaySequenceEditModel? GetDisplaySequence(int? categoryId)
        {
            if (categoryId is null || DisplaySequence is null)
                return null;

            return DisplaySequence.FirstOrDefault(i => i.ProductCategory?.Id == categoryId.Value);
        }
    }
}