using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Share
{
    /// <summary>
    /// Command to share a shop by cloning it into a new shop owned by another user.
    /// </summary>
    public class ShareCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the shop to share.
        /// </summary>
        public Guid ShopId { get; set; }

        /// <summary>
        /// Id of the user who will own the shared shop.
        /// </summary>
        public string TargetUserId { get; set; } = string.Empty;

        /// <summary>
        /// Bearer token of the current user, forwarded to RecipeBook.Api to resolve product categories.
        /// </summary>
        public string? AuthToken { get; set; }
    }
}
