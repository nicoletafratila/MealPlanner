using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingListController : ControllerBase
    {
        private readonly IShoppingListRepository _shoppingListRepository;
        private readonly IMealPlanRepository _meanPlanRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public ShoppingListController(IShoppingListRepository shoppingListRepository, IMealPlanRepository mealPlanRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _shoppingListRepository = shoppingListRepository;
            _meanPlanRepository = mealPlanRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<IList<ShoppingListModel>>> GetAll()
        {
            try
            {
                var results = await _shoppingListRepository.GetAllAsync();
                var mappedResults = _mapper.Map<IList<ShoppingListModel>>(results).OrderBy(item => item.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditShoppingListModel>> GetEdit(int id)
        {
            try
            {
                var result = await _shoppingListRepository.GetByIdIncludeProductsAsync(id);
                if (result == null) return NotFound();
                return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditShoppingListModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("makeshoppinglist/{id}")]
        public async Task<ActionResult<EditShoppingListModel>> GetShoppingListFromMealPlan(int id)
        {
            try
            {
                var mealPlan = await _meanPlanRepository.GetByIdIncludeRecipesAsync(id);
                if (mealPlan == null) return NotFound();

                var products = new List<ShoppingListProductModel>();
                foreach (var item in mealPlan.MealPlanRecipes!)
                {
                    foreach (var i in item.Recipe!.RecipeIngredients!)
                    {
                        var existingProduct = products.FirstOrDefault(x => x.Product!.Id == i.ProductId);
                        if (existingProduct == null)
                        {
                            var product = _mapper.Map<ShoppingListProductModel>(i);
                            product.Collected = false;
                            products.Add(product);
                        }
                        else
                            existingProduct.Quantity += i.Quantity;
                    }
                }

                var list = new EditShoppingListModel();
                list.Name = "List for " + mealPlan.Name;
                list.Products = products.OrderBy(i => i.Product!.ProductCategory!.DisplaySequence)
                               .Select(i => _mapper!.Map<ShoppingListProductModel>(i))
                               .ToList();

                var result = _mapper.Map<ShoppingList>(list);
                await _shoppingListRepository.AddAsync(result);

                result = await _shoppingListRepository.GetByIdIncludeProductsAsync(result.Id);
                string? location = _linkGenerator.GetPathByAction("GetEdit", "ShoppingList", new { id = result!.Id });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current id");
                }
                return Created(location, _mapper.Map<EditShoppingListModel>(result));

                //return await Post(list);
                //return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditShoppingListModel>(list));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        //[HttpPost]
        //public async Task<ActionResult<EditShoppingListModel>> Post(EditShoppingListModel model)
        //{
        //    if (model == null)
        //        return BadRequest();

        //    try
        //    {
        //        var result = _mapper.Map<ShoppingList>(model);
        //        await _shoppingListRepository.AddAsync(result);

        //        result = await _shoppingListRepository.GetByIdIncludeProductsAsync(result.Id);
        //        string? location = _linkGenerator.GetPathByAction("GetEdit", "ShoppingList", new { id = result!.Id });
        //        if (string.IsNullOrWhiteSpace(location))
        //        {
        //            return BadRequest("Could not use current id");
        //        }
        //        return Created(location, _mapper.Map<EditShoppingListModel>(result));
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
        //    }
        //}

        [HttpPut]
        public async Task<ActionResult> Put(EditShoppingListModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                var oldModel = await _shoppingListRepository.GetByIdAsync(model.Id);
                if (oldModel == null)
                {
                    return NotFound($"Could not find with id {model.Id}");
                }

                _mapper.Map(model, oldModel);
                await _shoppingListRepository.UpdateAsync(oldModel);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var itemToDelete = await _shoppingListRepository.GetByIdAsync(id);
            if (itemToDelete == null)
            {
                NotFound($"Could not find with id {id}");
                return;
            }

            await _shoppingListRepository.DeleteAsync(itemToDelete);
        }
    }
}
