using System.ComponentModel.DataAnnotations;
using Common.Models;
using Common.Validators;
using MealPlanner.Shared.Resources;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Editable model for creating/updating a shop, including product category display order.
    /// </summary>
    public class ShopEditModel : BaseModel
    {
        /// <summary>
        /// Database identity (empty for new shops).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Shop name (required, max 100 characters).
        /// </summary>
        [Required(ErrorMessageResourceName = nameof(MealPlannerSharedMessages.ShopNameRequired), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Shop image (optional). When not set, the UI falls back to a default image.
        /// </summary>
        [MaxLength(512000, ErrorMessageResourceName = nameof(RecipeBookSharedMessages.ImageTooLarge), ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
        public byte[]? ImageContent { get; set; }

        /// <summary>
        /// Full-size shop image URL (data URI), computed from <see cref="ImageContent"/>; null when no image is set.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Product category display sequence for this shop.
        /// Must contain at least one entry.
        /// </summary>
        [Required]
        [MinimumCountCollection(1, ErrorMessageResourceName = nameof(MealPlannerSharedMessages.ShopRequiresCategoryOrder), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
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
        public ShopDisplaySequenceEditModel? GetDisplaySequence(Guid? categoryId)
        {
            if (categoryId is null || DisplaySequence is null)
                return null;

            return DisplaySequence.FirstOrDefault(i => i.ProductCategory?.Id == categoryId.Value);
        }
    }
}
