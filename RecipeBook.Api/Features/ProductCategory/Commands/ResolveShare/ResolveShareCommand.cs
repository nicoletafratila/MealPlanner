using MediatR;

namespace RecipeBook.Api.Features.ProductCategory.Commands.ResolveShare
{
    /// <summary>
    /// Command to resolve (or create, if missing) the target user's equivalents of a set of product categories,
    /// by matching on name.
    /// </summary>
    public class ResolveShareCommand : IRequest<Dictionary<Guid, Guid>>
    {
        /// <summary>
        /// Ids of the source product categories to resolve.
        /// </summary>
        public IList<Guid> CategoryIds { get; set; } = [];

        /// <summary>
        /// Id of the user who will own the resolved categories.
        /// </summary>
        public string TargetUserId { get; set; } = string.Empty;
    }
}
