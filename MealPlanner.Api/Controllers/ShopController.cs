using MealPlanner.Api.Features.Shop.Commands.AddShop;
using MealPlanner.Api.Features.Shop.Commands.DeleteShop;
using MealPlanner.Api.Features.Shop.Commands.UpdateShop;
using MealPlanner.Api.Features.Shop.Queries.GetEditShop;
using MealPlanner.Api.Features.Shop.Queries.GetShops;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet("edit/{id:int}")]
        public async Task<EditShopModel> GetEdit(int id)
        {
            GetEditShopQuery query = new()
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

        [HttpPost]
        public async Task<AddShopCommandResponse> Post(EditShopModel model)
        {
            AddShopCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateShopCommandResponse> Put(EditShopModel model)
        {
            UpdateShopCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteShopCommandResponse> Delete(int id)
        {
            DeleteShopCommand command = new()
            {
                Id = id
            };
            return await _mediator.Send(command);
        }
    }
}
