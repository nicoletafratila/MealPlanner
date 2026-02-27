using Common.Models;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Lightweight model representing a meal plan.
    /// </summary>
    public sealed class MealPlanModel : BaseModel
    {
        /// <summary>
        /// Database identity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Meal plan name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public MealPlanModel()
        {
        }

        public MealPlanModel(int id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }
}