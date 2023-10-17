using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.ProductCategory.Queries.GetProductCategory;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly ISender _mediator;

        public ProductCategoryController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IList<ProductCategoryModel>> GetAll()
        {
            return await _mediator.Send(new GetProductCategoryQuery());
        }
    }
}
