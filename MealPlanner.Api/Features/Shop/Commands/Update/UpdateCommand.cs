﻿using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Update
{
    public class UpdateCommand : IRequest<UpdateCommandResponse>
    {
        public EditShopModel? Model { get; set; }
    }
}
