using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetUnit
{
    public class GetUnitQuery : IRequest<IList<UnitModel>>
    {
    }
}
