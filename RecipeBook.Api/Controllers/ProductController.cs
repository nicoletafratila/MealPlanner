using System.Text.Json;
using BlazorBootstrap;
using Common.Models;
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
        [HttpGet("edit/{id:int}")]
        public async Task<ProductEditModel> GetEditAsync(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<ProductModel>> SearchAsync([FromQuery] string? filters, [FromQuery] string? sortString, [FromQuery] string? sortDirection, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            SearchQuery query = new()
            {
                QueryParameters = new QueryParameters()
                {
                    Filters = !string.IsNullOrWhiteSpace(filters) ? JsonSerializer.Deserialize<IEnumerable<FilterItem>>(filters) : null,
                    SortString = sortString,
                    SortDirection = sortDirection == SortDirection.Ascending.ToString() ? SortDirection.Ascending : SortDirection.Descending,
                    PageSize = int.Parse(pageSize!),
                    PageNumber = int.Parse(pageNumber!)
                }
            };
            return await mediator.Send(query);
        }

        [HttpPost]
        public async Task<CommandResponse> PostAsync(ProductEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut]
        public async Task<CommandResponse> PutAsync(ProductEditModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<CommandResponse> DeleteAsync(int id)
        {
            DeleteCommand command = new()
            {
                Id = id
            };
            return await mediator.Send(command);
        }
    }
}
