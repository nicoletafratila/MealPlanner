﻿using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Update
{
    public class UpdateCommand : IRequest<UpdateCommandResponse>
    {
        public ShoppingListEditModel? Model { get; set; }
    }
}
