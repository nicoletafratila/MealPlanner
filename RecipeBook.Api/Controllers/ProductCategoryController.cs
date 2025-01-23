using BlazorBootstrap;
using System.Text.Json;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.ProductCategory.Commands.Add;
using RecipeBook.Api.Features.ProductCategory.Commands.Delete;
using RecipeBook.Api.Features.ProductCategory.Commands.Update;
using RecipeBook.Api.Features.ProductCategory.Queries.GetEdit;
using RecipeBook.Api.Features.ProductCategory.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet("edit/{id:int}")]
        public async Task<ProductCategoryEditModel> GetEdit(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<ProductCategoryModel>> Search([FromQuery] string? filters, [FromQuery] string? sortString, [FromQuery] string? sortDirection, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
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
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<AddCommandResponse> Post(ProductCategoryEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateCommandResponse> Put(ProductCategoryEditModel model)
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
