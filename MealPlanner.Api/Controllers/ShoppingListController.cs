﻿using BlazorBootstrap;
using System.Text.Json;
using Common.Pagination;
using MealPlanner.Api.Features.ShoppingList.Commands.Add;
using MealPlanner.Api.Features.ShoppingList.Commands.Delete;
using MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList;
using MealPlanner.Api.Features.ShoppingList.Commands.Update;
using MealPlanner.Api.Features.ShoppingList.Queries.GetEdit;
using MealPlanner.Api.Features.ShoppingList.Queries.Search;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingListController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet("edit/{id:int}")]
        public async Task<ShoppingListEditModel> GetEdit(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<ShoppingListModel>> Search([FromQuery] string? filters, [FromQuery] string? sortString, [FromQuery] string? sortDirection, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            SearchQuery query = new()
            {
                QueryParameters = new QueryParameters()
                {
                    Filters = !string.IsNullOrWhiteSpace(filters) ? JsonSerializer.Deserialize<IEnumerable<FilterItem>>(filters) : null,
                    SortString = sortString,
                    SortDirection = sortDirection == SortDirection.Ascending.ToString() ? SortDirection.Ascending : SortDirection.Descending,
                    PageSize = int.Parse(pageSize),
                    PageNumber = int.Parse(pageNumber)
                }
            };
            return await _mediator.Send(query);
        }

        [HttpPost("makeShoppingList")]
        public async Task<ShoppingListEditModel?> MakeShoppingList(ShoppingListCreateModel model)
        {
            MakeShoppingListCommand command = new()
            {
                MealPlanId = model.MealPlanId,
                ShopId = model.ShopId
            };
            return await _mediator.Send(command);
        }

        [HttpPost]
        public async Task<AddCommandResponse> Post(ShoppingListEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateCommandResponse> Put(ShoppingListEditModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteCommandResponse> Delete(int id)
        {
            DeleteCommand command = new()
            {
                Id = id
            };
            return await _mediator.Send(command);
        }
    }
}
