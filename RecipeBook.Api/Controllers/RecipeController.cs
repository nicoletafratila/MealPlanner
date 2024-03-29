﻿using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Recipe.Commands.AddRecipe;
using RecipeBook.Api.Features.Recipe.Commands.DeleteRecipe;
using RecipeBook.Api.Features.Recipe.Commands.UpdateRecipe;
using RecipeBook.Api.Features.Recipe.Queries.GetEditRecipe;
using RecipeBook.Api.Features.Recipe.Queries.GetRecipe;
using RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts;
using RecipeBook.Api.Features.Recipe.Queries.SearchRecipes;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet("{id:int}")]
        public async Task<RecipeModel> GetById(int id)
        {
            GetRecipeQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("edit/{id:int}")]
        public async Task<EditRecipeModel> GetEdit(int id)
        {
            GetEditRecipeQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("shoppingListProducts/{recipeId:int}/{shopId:int}")]
        public async Task<IList<ShoppingListProductModel>?> GetShoppingListProducts(int recipeId, int shopId)
        {
            GetShoppingListProductsQuery query = new()
            {
                RecipeId = recipeId,
                ShopId = shopId
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<RecipeModel>> Search([FromQuery] string? categoryId, [FromQuery] QueryParameters? queryParameters)
        {
            SearchRecipesQuery query = new()
            {
                CategoryId = categoryId,
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<AddRecipeCommandResponse> PostAsync(EditRecipeModel model)
        {
            AddRecipeCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateRecipeCommandResponse> Put(EditRecipeModel model)
        {
            UpdateRecipeCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteRecipeCommandResponse> Delete(int id)
        {
            DeleteRecipeCommand command = new()
            {
                Id = id
            };
            return await _mediator.Send(command);
        }
    }
}
