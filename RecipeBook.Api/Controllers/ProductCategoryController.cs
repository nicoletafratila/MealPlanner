using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.ProductCategory.Queries.GetProductCategories;
using RecipeBook.Api.Features.ProductCategory.Queries.SearchProductCategory;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet]
        public async Task<IList<ProductCategoryModel>> GetAll()
        {
            return await _mediator.Send(new GetProductCategoriesQuery());
        }

        [HttpGet("search")]
        public async Task<PagedList<ProductCategoryModel>> Search([FromQuery] QueryParameters? queryParameters)
        {
            SearchProductCategoryQuery query = new()
            {
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }
    }
}
