using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetRecipeCategories;
using RecipeBook.Api.Features.RecipeCategory.Queries.SearchRecipeCategory;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeCategoryController : ControllerBase
    {
        private readonly ISender _mediator;

        public RecipeCategoryController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IList<RecipeCategoryModel>> GetAll()
        {
            return await _mediator.Send(new GetRecipeCategoriesQuery());
        }

        [HttpGet("search")]
        public async Task<PagedList<RecipeCategoryModel>> Search([FromQuery] QueryParameters? queryParameters)
        {
            SearchRecipeCategoryQuery query = new()
            {
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }
    }
}
