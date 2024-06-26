﻿using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public ShoppingListEditModel? Model { get; set; }
    }
}
