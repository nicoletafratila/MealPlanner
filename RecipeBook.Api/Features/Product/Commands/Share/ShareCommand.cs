using Common.Models;
using MediatR;

namespace RecipeBook.Api.Features.Product.Commands.Share
{
    /// <summary>
    /// Command to share a product by cloning it into a new product owned by another user.
    /// </summary>
    public class ShareCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the product to share.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Id of the user who will own the shared product.
        /// </summary>
        public string TargetUserId { get; set; } = string.Empty;
    }
}
