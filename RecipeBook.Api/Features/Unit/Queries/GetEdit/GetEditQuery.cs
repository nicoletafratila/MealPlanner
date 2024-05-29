using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetEdit
{ 
    public class GetEditQuery : IRequest<UnitEditModel>
    {
        public int Id { get; set; }
    }
}
