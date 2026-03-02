using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetEdit
{
    /// <summary>
    /// Query to retrieve a unit for editing.
    /// </summary>
    public class GetEditQuery : IRequest<UnitEditModel>
    {
        /// <summary>
        /// Id of the unit to edit.
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