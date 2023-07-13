using AutoMapper;
using Common.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public ProductController(IProductRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<IList<ProductModel>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                var mappedResult = _mapper.Map<IList<ProductModel>>(result).OrderBy(item => item.ProductCategory!.DisplaySequence).ThenBy(item => item.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResult);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EditProductModel>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            try
            {
                var result = await _repository.GetByIdAsync(id);
                if (result == null) return NotFound();
                return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditProductModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("category/{categoryid:int}")]
        public async Task<ActionResult<IList<ProductModel>>> Search(int categoryId)
        {
            if (categoryId <= 0)
                return BadRequest();

            try
            {
                var results = await _repository.SearchAsync(categoryId);
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

            var existingItem = await _repository.SearchAsync(model.Name);
            if (existingItem != null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            try
            {
                var result = _mapper.Map<Product>(model);
                await _repository.AddAsync(result);

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
                var oldModel = await _repository.GetByIdAsync(model.Id);
                if (oldModel == null)
                {
                    return NotFound($"Could not find with id {model.Id}");
                }

                _mapper.Map(model, oldModel);
                await _repository.UpdateAsync(oldModel);
                return _mapper.Map<EditProductModel>(oldModel);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var itemToDelete = await _repository.GetByIdAsync(id);
            if (itemToDelete == null)
            {
                NotFound($"Could not find with id {id}");
                return;
            }

            await _repository.DeleteAsync(itemToDelete);
        }
    }
}
