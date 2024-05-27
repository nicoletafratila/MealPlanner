using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetAll;
using RecipeBook.Api.Features.RecipeCategory.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeCategoryController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

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
    }
}
