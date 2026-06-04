using Common.Models;
using MediatR;

namespace RecipeBook.Api.Features.Recipe.Commands.Delete
{
    /// <summary>
    /// Command to delete a recipe by id.
    /// </summary>
    public class DeleteCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the recipe to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Optional auth token if downstream services require it.
        /// </summary>
        public string? AuthToken { get; set; }

        public DeleteCommand()
        {
        }

        public DeleteCommand(Guid id, string? authToken = null)
        {
            Id = id;
            AuthToken = authToken;
        }
    }
}