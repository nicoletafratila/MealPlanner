using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Features.ShoppingList.Queries.SearchShoppingLists;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;
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
        private readonly ISender _mediator;

        public ShoppingListController(ISender mediator, IShoppingListRepository shoppingListRepository, IMealPlanRepository mealPlanRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _shoppingListRepository = shoppingListRepository;
            _meanPlanRepository = mealPlanRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
            _mediator = mediator;
        }

        [HttpGet("edit/{id:int}")]
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

        [HttpGet("search")]
        public async Task<PagedList<ShoppingListModel>> Search([FromQuery] QueryParameters? queryParameters)
        {
            SearchShoppingListsQuery query = new()
            {
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<ActionResult<EditShoppingListModel>> Post([FromBody] int id)
        {
            try
            {
                var mealPlan = await _meanPlanRepository.GetByIdIncludeRecipesAsync(id);
                if (mealPlan == null)
                {
                    return NotFound();
                }

                var list = mealPlan.GetShoppingList();
                await _shoppingListRepository.AddAsync(list);

                var result = await _shoppingListRepository.GetByIdIncludeProductsAsync(list.Id);
                string? location = _linkGenerator.GetPathByAction("GetEdit", "ShoppingList", new { id = result!.Id });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current id");
                }
                return Created(location, _mapper.Map<EditShoppingListModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(EditShoppingListModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                var oldModel = await _shoppingListRepository.GetByIdIncludeProductsAsync(model.Id);
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
