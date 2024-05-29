using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.RecipeCategory.Commands.Add;
using RecipeBook.Api.Features.RecipeCategory.Commands.Delete;
using RecipeBook.Api.Features.RecipeCategory.Commands.Update;
using RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetAll;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit;
using RecipeBook.Api.Features.RecipeCategory.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeCategoryController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet("edit/{id:int}")]
        public async Task<RecipeCategoryEditModel> GetEdit(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet]
        public async Task<IList<RecipeCategoryModel>> GetAll()
        {
            return await _mediator.Send(new GetAllQuery());
        }

        [HttpGet("search")]
        public async Task<PagedList<RecipeCategoryModel>> Search([FromQuery] QueryParameters? queryParameters)
        {
            SearchQuery query = new()
            {
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<AddCommandResponse> Post(RecipeCategoryEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateCommandResponse> Put(RecipeCategoryEditModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut("updateAll")]
        public async Task<UpdateAllCommandResponse> Put(IList<RecipeCategoryModel> models)
        {
            UpdateAllCommand command = new()
            {
                Models = models
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
