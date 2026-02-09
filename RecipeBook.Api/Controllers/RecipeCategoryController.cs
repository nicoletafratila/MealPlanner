using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RecipeBook.Api.Features.RecipeCategory.Commands.Add;
using RecipeBook.Api.Features.RecipeCategory.Commands.Delete;
using RecipeBook.Api.Features.RecipeCategory.Commands.Update;
using RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit;
using RecipeBook.Api.Features.RecipeCategory.Queries.Search;
using RecipeBook.Api.Features.RecipeCategory.Queries.SearchByCategories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class RecipeCategoryController(ISender mediator) : ControllerBase
    {
        [HttpGet("edit")]
        public async Task<RecipeCategoryEditModel> GetEditAsync(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<RecipeCategoryModel>> SearchAsync([FromQuery] string? filters, [FromQuery] string? sorting, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            SearchQuery query = new()
            {
                QueryParameters = new QueryParameters<RecipeCategoryModel>()
                {
                    Filters = !string.IsNullOrWhiteSpace(filters) ? JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(filters) : null,
                    Sorting = !string.IsNullOrWhiteSpace(sorting) ? JsonConvert.DeserializeObject<IEnumerable<SortingModel>>(sorting) : null,
                    PageSize = int.Parse(pageSize!),
                    PageNumber = int.Parse(pageNumber!)
                }
            };
            return await mediator.Send(query);
        }

        [HttpGet("searchbycategories")]
        public async Task<IList<RecipeCategoryModel>> SearchAsync([FromQuery] string categoryIds)
        {
            SearchByCategoriesQuery query = new()
            {
                CategoryIds = categoryIds
            };
            return await mediator.Send(query);
        }

        [HttpPost]
        public async Task<CommandResponse?> PostAsync(RecipeCategoryEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut]
        public async Task<CommandResponse?> PutAsync(RecipeCategoryEditModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut("updateAll")]
        public async Task<CommandResponse?> PutAsync(IList<RecipeCategoryModel> models)
        {
            UpdateAllCommand command = new()
            {
                Models = models
            };
            return await mediator.Send(command);
        }

        [HttpDelete]
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
