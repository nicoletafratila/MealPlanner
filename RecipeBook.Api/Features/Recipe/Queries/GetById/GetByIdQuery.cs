using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetById
{
    /// <summary>
    /// Query to retrieve a recipe by id (read-only model).
    /// </summary>
    public class GetByIdQuery : IRequest<RecipeModel>
    {
        /// <summary>
        /// Id of the recipe to retrieve.
        /// </summary>
        public int Id { get; set; }

        public GetByIdQuery()
        {
        }

        public GetByIdQuery(int id)
        {
            Id = id;
        }
    }
}