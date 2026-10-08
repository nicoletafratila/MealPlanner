using Common.Pagination;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Request body to share all of the current user's shops by cloning them into new shops owned by another user.
    /// </summary>
    public class ShopShareAllModel
    {
        public string TargetUserId { get; set; } = string.Empty;

        /// <summary>
        /// When provided, only shops matching these filters (the same ones applied to the shops grid) are shared.
        /// </summary>
        public IEnumerable<FilterItem>? Filters { get; set; }
    }
}
