using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetAll
{
    public class GetAllQuery : IRequest<IList<UnitModel>>
    {
    }
}
