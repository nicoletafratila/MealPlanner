using Common.Models;
using MediatR;

namespace RecipeBook.Api.Features.Recipe.Commands.Share
{
    /// <summary>
    /// Command to share a recipe by cloning it into a new recipe owned by another user.
    /// </summary>
    public class ShareCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the recipe to share.
        /// </summary>
        public Guid RecipeId { get; set; }

        /// <summary>
        /// Id of the user who will own the shared recipe.
        /// </summary>
        public string TargetUserId { get; set; } = string.Empty;
    }
}
