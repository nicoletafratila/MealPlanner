﻿using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public MealPlanEditModel? Model { get; set; }
    }
}
