using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEdit
{
    /// <summary>
    /// Query to retrieve a recipe for editing.
    /// </summary>
    public class GetEditQuery : IRequest<RecipeEditModel>
    {
        /// <summary>
        /// Id of the recipe to edit.
        /// </summary>
        public int Id { get; set; }

        public GetEditQuery()
        {
        }

        public GetEditQuery(int id)
        {
            Id = id;
        }
    }
}