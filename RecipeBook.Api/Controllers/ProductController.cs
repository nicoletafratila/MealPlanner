using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Product.Commands.Add;
using RecipeBook.Api.Features.Product.Commands.Delete;
using RecipeBook.Api.Features.Product.Commands.Update;
using RecipeBook.Api.Features.Product.Queries.GetEdit;
using RecipeBook.Api.Features.Product.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet("edit/{id:int}")]
        public async Task<ProductEditModel> GetEdit(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<ProductModel>> Search([FromQuery] string? categoryId, [FromQuery] QueryParameters? queryParameters)
        {
            SearchQuery query = new()
            {
                CategoryId = categoryId,
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<AddCommandResponse> PostAsync(ProductEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateCommandResponse> Put(ProductEditModel model)
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
