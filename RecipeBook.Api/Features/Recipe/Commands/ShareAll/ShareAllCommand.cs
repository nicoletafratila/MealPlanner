using Common.Models;
using MediatR;

namespace RecipeBook.Api.Features.Recipe.Commands.ShareAll
{
    /// <summary>
    /// Command to share all of the current user's recipes by cloning them, and the products they reference, into new records owned by another user.
    /// </summary>
    public class ShareAllCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the user who will own the shared recipes.
        /// </summary>
        public string TargetUserId { get; set; } = string.Empty;
    }
}
