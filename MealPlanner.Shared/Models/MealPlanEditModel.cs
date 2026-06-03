using System.ComponentModel.DataAnnotations;using Common.Models; using Common.Validators; using MealPlanner.Shared.Resources; using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Editable model for creating/updating a meal plan.
    /// </summary>
    public class MealPlanEditModel : BaseModel
    {
        /// <summary>
        /// Database identity (empty for new plans).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Meal plan name (required, max 100 characters).
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Recipes included in this meal plan.
        /// Must contain at least one recipe.
        /// </summary>
        [Required]
        [MinimumCountCollection(1, ErrorMessageResourceName = nameof(MealPlannerSharedMessages.MealPlanRequiresRecipes), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
        public IList<RecipeModel>? Recipes { get; set; } = [];

        public MealPlanEditModel()
        {
        }

        public MealPlanEditModel(Guid id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }
}
