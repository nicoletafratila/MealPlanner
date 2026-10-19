using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Commands.ShareAll
{
    /// <summary>
    /// Command to queue a background job that shares all of the current user's recipes by cloning them, and the
    /// products they reference, into new records owned by another user.
    /// </summary>
    public class StartShareAllCommand : IRequest<JobStartedModel?>
    {
        /// <summary>
        /// Id of the user who will own the shared recipes.
        /// </summary>
        public string TargetUserId { get; set; } = string.Empty;

        /// <summary>
        /// When provided, only recipes matching these filters (the same ones applied to the recipes grid) are shared.
        /// </summary>
        public IEnumerable<FilterItem>? Filters { get; set; }
    }
}
