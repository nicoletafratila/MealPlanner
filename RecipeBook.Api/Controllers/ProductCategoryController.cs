using Common.Pagination;
using MealPlanner.Api.Features.ProductCategory.Commands.AddProductCategory;
using MealPlanner.Api.Features.ProductCategory.Commands.DeleteProductCategory;
using MealPlanner.Api.Features.ProductCategory.Commands.ProductCategory;
using MealPlanner.Api.Features.ProductCategory.Commands.UpdateProductCategory;
using MealPlanner.Api.Features.ProductCategory.Queries.GetEditProductCategory;
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

        [HttpGet("edit/{id:int}")]
        public async Task<EditProductCategoryModel> GetEdit(int id)
        {
            GetEditProductCategoryQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

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

        [HttpPost]
        public async Task<AddProductCategoryCommandResponse> Post(EditProductCategoryModel model)
        {
            AddProductCategoryCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateProductCategoryCommandResponse> Put(EditProductCategoryModel model)
        {
            UpdateProductCategoryCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteProductCategoryCommandResponse> Delete(int id)
        {
            DeleteProductCategoryCommand command = new()
            {
                Id = id
            };
            return await _mediator.Send(command);
        }
    }
}
