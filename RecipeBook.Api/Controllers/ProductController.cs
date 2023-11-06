using AutoMapper;
using Common.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Product.Queries.GetEditProduct;
using RecipeBook.Api.Features.Product.Queries.GetProducts;
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
        private readonly LinkGenerator _linkGenerator;
        private readonly ISender _mediator;

        public ProductController(ISender mediator, IProductRepository productRepository, IRecipeIngredientRepository recipeIngredientRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
            _recipeIngredientRepository = recipeIngredientRepository;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IList<ProductModel>> GetAll()
        {
            return await _mediator.Send(new GetProductsQuery());
        }

        [HttpGet("edit/{id:int}")]
        public async Task<ActionResult<EditProductModel>> GetEdit(int id)
        {
            var query = new GetEditProductQuery();
            query.Id = id;

            return await _mediator.Send(query);
        }

        [HttpGet("search/{categoryid:int}")]
        public async Task<ActionResult<IList<ProductModel>>> Search(int categoryId)
        {
            if (categoryId <= 0)
                return BadRequest();

            try
            {
                var results = await _productRepository.SearchAsync(categoryId);
                var mappedResults = _mapper.Map<IList<ProductModel>>(results).OrderBy(item => item.ProductCategory!.DisplaySequence).ThenBy(item => item.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<EditProductModel>> Post(EditProductModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
                return BadRequest();

            var existingItem = await _productRepository.SearchAsync(model.Name);
            if (existingItem != null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            try
            {
                var result = _mapper.Map<Product>(model);
                await _productRepository.AddAsync(result);

                string? location = _linkGenerator.GetPathByAction("GetById", "Recipe", new { id = result.Id });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current id");
                }
                return Created(location, _mapper.Map<EditProductModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
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
