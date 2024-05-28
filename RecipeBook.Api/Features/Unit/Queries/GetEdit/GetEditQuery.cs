using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetEdit
{ 
    public class GetEditQuery : IRequest<EditUnitModel>
    {
        public int Id { get; set; }
    }
}
