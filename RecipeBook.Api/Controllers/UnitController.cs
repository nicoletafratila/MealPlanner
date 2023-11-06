using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Unit.Queries.GetUnit;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController : ControllerBase
    {
        private readonly ISender _mediator;

        public UnitController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IList<UnitModel>> GetAll()
        {
            return await _mediator.Send(new GetUnitQuery());
        }
    }
}
