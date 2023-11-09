using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Product.Commands.AddProduct;
using RecipeBook.Api.Features.Product.Commands.DeleteProduct;
using RecipeBook.Api.Features.Product.Commands.UpdateProduct;
using RecipeBook.Api.Features.Product.Queries.GetEditProduct;
using RecipeBook.Api.Features.Product.Queries.SearchProducts;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ISender _mediator;

        public ProductController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("edit/{id:int}")]
        public async Task<EditProductModel> GetEdit(int id)
        {
            GetEditProductQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<ProductModel>> Search([FromQuery] string? categoryId, [FromQuery] QueryParameters? queryParameters)
        {
            SearchProductsQuery query = new()
            {
                CategoryId = categoryId,
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<AddProductCommandResponse> PostAsync(EditProductModel model)
        {
            AddProductCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateProductCommandResponse> Put(EditProductModel model)
        {
            UpdateProductCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteProductCommandResponse> Delete(int id)
        {
            DeleteProductCommand command = new()
            {
                Id = id
            };
            return await _mediator.Send(command);
        }
    }
}
