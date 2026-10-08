namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Request body to share a shop by cloning it into a new shop owned by another user.
    /// </summary>
    public class ShopShareModel
    {
        public Guid ShopId { get; set; }

        public string TargetUserId { get; set; } = string.Empty;
    }
}
