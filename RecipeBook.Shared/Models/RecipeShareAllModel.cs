using Common.Pagination;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Request body to share all of the current user's recipes by cloning them into new recipes owned by another user.
    /// </summary>
    public class RecipeShareAllModel
    {
        public string TargetUserId { get; set; } = string.Empty;

        /// <summary>
        /// When provided, only recipes matching these filters (the same ones applied to the recipes grid) are shared.
        /// </summary>
        public IEnumerable<FilterItem>? Filters { get; set; }
    }
}
