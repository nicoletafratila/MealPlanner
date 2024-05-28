using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Unit.Commands.Add;
using RecipeBook.Api.Features.Unit.Commands.Delete;
using RecipeBook.Api.Features.Unit.Commands.Update;
using RecipeBook.Api.Features.Unit.Queries.GetAll;
using RecipeBook.Api.Features.Unit.Queries.GetEdit;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet("edit/{id:int}")]
        public async Task<EditUnitModel> GetEdit(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet]
        public async Task<IList<UnitModel>> GetAll()
        {
            return await _mediator.Send(new GetAllQuery());
        }

        [HttpPost]
        public async Task<AddCommandResponse> Post(EditUnitModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateCommandResponse> Put(EditUnitModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteCommandResponse> Delete(int id)
        {
            DeleteCommand command = new()
            {
                Id = id
            };
            return await _mediator.Send(command);
        }
    }
}
