using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
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
        public async Task<PagedList<ProductModel>> SearchAsync([FromQuery] string? filters, [FromQuery] string? sorting, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            SearchQuery query = new()
            {
                QueryParameters = new QueryParameters<ProductModel>()
                {
                    Filters = !string.IsNullOrWhiteSpace(filters) ? JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(filters) : null,
                    Sorting = !string.IsNullOrWhiteSpace(sorting) ? JsonConvert.DeserializeObject<IEnumerable<SortingModel>>(sorting) : null,
                    PageSize = int.Parse(pageSize!),
                    PageNumber = int.Parse(pageNumber!)
                }
            };
            return await mediator.Send(query);
        }

        [HttpPost]
        public async Task<CommandResponse?> PostAsync(ProductEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut]
        public async Task<CommandResponse?> PutAsync(ProductEditModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            DeleteCommand command = new()
            {
                Id = id
            };
            return await mediator.Send(command);
        }
    }
}
