using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetUnits
{
    public class GetUnitsQuery : IRequest<IList<UnitModel>>
    {
    }
}
