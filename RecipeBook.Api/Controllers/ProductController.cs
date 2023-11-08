using AutoMapper;
using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Product.Commands.AddProduct;
using RecipeBook.Api.Features.Product.Queries.GetEditProduct;
using RecipeBook.Api.Features.Product.Queries.SearchProducts;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IRecipeIngredientRepository _recipeIngredientRepository;
        private readonly IMapper _mapper;
        private readonly ISender _mediator;

        public ProductController(ISender mediator, IProductRepository productRepository, IRecipeIngredientRepository recipeIngredientRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _recipeIngredientRepository = recipeIngredientRepository;
            _mediator = mediator;
        }

        [HttpGet("edit/{id:int}")]
        public async Task<EditProductModel> GetEdit(int id)
        {
            GetEditProductQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<ProductModel>> Search([FromQuery] string? categoryId, [FromQuery] QueryParameters? queryParameters)
        {
            SearchProductsQuery query = new()
            {
                CategoryId = categoryId,
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }


        [HttpPost("")]
        public async Task<AddProductCommandResponse> PostAsync(AddProductCommand addQuestionCommand)
        {
            return await _mediator.Send(addQuestionCommand);
        }

        [HttpPut]
        public async Task<ActionResult<EditProductModel>> Put(EditProductModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                var oldModel = await _productRepository.GetByIdAsync(model.Id);
                if (oldModel == null)
                {
                    return NotFound($"Could not find with id {model.Id}");
                }

                _mapper.Map(model, oldModel);
                await _productRepository.UpdateAsync(oldModel);
                return _mapper.Map<EditProductModel>(oldModel);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var itemToDelete = await _productRepository.GetByIdAsync(id);
            if (itemToDelete == null)
            {
                return NotFound($"Could not find with id {id}");
            }

            var result = await _recipeIngredientRepository.SearchAsync(id);
            if (result != null && result.Any())
            {
                return BadRequest($"The product you try to delete is used in recipes and cannot be deleted.");
            }

            await _productRepository.DeleteAsync(itemToDelete!);
            return StatusCode(StatusCodes.Status200OK);
        }
    }
}
