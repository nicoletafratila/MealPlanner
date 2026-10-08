using Common.Models;
using Common.Pagination;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.ShareAll
{
    /// <summary>
    /// Command to share all of the current user's shops by cloning them, and their display sequences, into new
    /// records owned by another user.
    /// </summary>
    public class ShareAllCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the user who will own the shared shops.
        /// </summary>
        public string TargetUserId { get; set; } = string.Empty;

        /// <summary>
        /// When provided, only shops matching these filters (the same ones applied to the shops grid) are shared.
        /// </summary>
        public IEnumerable<FilterItem>? Filters { get; set; }

        /// <summary>
        /// Bearer token of the current user, forwarded to RecipeBook.Api to resolve product categories.
        /// </summary>
        public string? AuthToken { get; set; }
    }
}
