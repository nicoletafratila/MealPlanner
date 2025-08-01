using System.Text.Json;
using BlazorBootstrap;
using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.RecipeCategory.Commands.Add;
using RecipeBook.Api.Features.RecipeCategory.Commands.Delete;
using RecipeBook.Api.Features.RecipeCategory.Commands.Update;
using RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit;
using RecipeBook.Api.Features.RecipeCategory.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeCategoryController(ISender mediator) : ControllerBase
    {
        [HttpGet("edit/{id:int}")]
        public async Task<RecipeCategoryEditModel> GetEditAsync(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<RecipeCategoryModel>> SearchAsync([FromQuery] string? filters, [FromQuery] string? sortString, [FromQuery] string? sortDirection, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
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
        public async Task<AddCommandResponse> PostAsync(RecipeCategoryEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateCommandResponse> PutAsync(RecipeCategoryEditModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut("updateAll")]
        public async Task<UpdateAllCommandResponse> PutAsync(IList<RecipeCategoryModel> models)
        {
            UpdateAllCommand command = new()
            {
                Models = models
            };
            return await mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteCommandResponse> DeleteAsync(int id)
        {
            DeleteCommand command = new()
            {
                Id = id
            };
            return await mediator.Send(command);
        }
    }
}
