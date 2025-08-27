using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using MealPlanner.Api.Features.Shop.Commands.Add;
using MealPlanner.Api.Features.Shop.Commands.Delete;
using MealPlanner.Api.Features.Shop.Commands.Update;
using MealPlanner.Api.Features.Shop.Queries.GetEdit;
using MealPlanner.Api.Features.Shop.Queries.Search;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShopController(ISender mediator) : ControllerBase
    {
        [HttpGet("edit/{id:int}")]
        public async Task<ShopEditModel> GetEditAsync(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<ShopModel>> SearchAsync([FromQuery] string? filters, [FromQuery] string? sortString, [FromQuery] string? sortDirection, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            SearchQuery query = new()
            {
                QueryParameters = new QueryParameters()
                {
                    Filters = !string.IsNullOrWhiteSpace(filters) ? JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(filters) : null,
                    SortString = sortString,
                    SortDirection = sortDirection == SortDirection.Ascending.ToString() ? SortDirection.Ascending : SortDirection.Descending,
                    PageSize = int.Parse(pageSize!),
                    PageNumber = int.Parse(pageNumber!)
                }
            };
            return await mediator.Send(query);
        }

        [HttpPost]
        public async Task<CommandResponse?> PostAsync(ShopEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut]
        public async Task<CommandResponse?> PutAsync(ShopEditModel model)
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
