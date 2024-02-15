using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Unit.Queries.GetUnits;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet]
        public async Task<IList<UnitModel>> GetAll()
        {
            return await _mediator.Send(new GetUnitsQuery());
        }
    }
}
