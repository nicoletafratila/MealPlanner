using MealPlanner.Api.Features.Shop.Queries.GetShop;
using MealPlanner.Api.Features.Shop.Queries.GetShops;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly ISender _mediator;

        public ShopController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:int}")]
        public async Task<ShopModel> GetById(int id)
        {
            GetShopQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet]
        public async Task<IList<ShopModel>> GetAll()
        {
            return await _mediator.Send(new GetShopsQuery());
        }
    }
}
